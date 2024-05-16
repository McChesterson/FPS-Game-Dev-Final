using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager instance;

    [SerializeField] private float respawnTime = 3f;

    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    public Dictionary<int, Player> _players = new Dictionary<int, Player>();
    private List<int> _deadPlayers = new List<int>();

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsServerStarted)
        {
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        for(int i = 0; i < _deadPlayers.Count; i++)
        {
            if (_players[_deadPlayers[i]].deathTime < Time.time - respawnTime)
            {
                RespawnPlayer(_deadPlayers[i]);
                _deadPlayers.RemoveAt(i);
                return;
            }
        }
    }

    private void RespawnPlayer(int cliendID)
    {
        PlayerController.SetPlayerPosition(cliendID, spawnPoints[Random.Range(0, spawnPoints.Count)].position);

        PlayerController.TogglePlayer(cliendID, true);
        if (PlayerHealth.Players.TryGetValue(cliendID, out PlayerHealth _playerHealth))
            _playerHealth.ResetHealth();
    }

    public static void InitializeNewPlayer(int clientID)
    {
        instance._players.Add(clientID, new Player());
    }

    public static void PlayerDisconnected(int clientID)
    {
        instance._players.Remove(clientID);
    }

    public static void PlayerDied(int player, int killer)
    {
        if(instance._players.TryGetValue(killer, out Player killerPlayer))
        {
            killerPlayer.score++;
            Debug.Log($"{killerPlayer.playerName} score = {killerPlayer.score}");
        }
        if (instance._players.TryGetValue(player, out Player deadPlayer))
        {
            deadPlayer.deaths++;
            Debug.Log($"{deadPlayer.playerName} has been killed {deadPlayer.deaths} times");
            deadPlayer.deathTime = Time.time;
        }


        GameUIManager.SetKills(killer, killerPlayer.score);
        GameUIManager.SetDeaths(player, deadPlayer.deaths);
        instance._deadPlayers.Add(player);
    }

    public class Player
    {
        private List<string> regularNames = new List<string>() { "Jeff", "Bob", "Billy", "Joe", "That One Guy", "Yoink" };

        public string playerName = null;

        public int score = 0;
        public int deaths = 0;

        public float deathTime = -99;

        public Player()
        {
            playerName = regularNames[Random.Range(0, regularNames.Count)];
        }
    }
}
