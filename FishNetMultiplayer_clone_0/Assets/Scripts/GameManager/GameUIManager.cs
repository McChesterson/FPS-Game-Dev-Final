using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class GameUIManager : NetworkBehaviour
{
    private static GameUIManager instance;

    [SerializeField] private TextMeshProUGUI playerName;

    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Scorecard")]
    [SerializeField] private GameObject scoreboard;
    [SerializeField] private ScoreboardCard cardPrefab;
    [SerializeField] private Transform cardParent;

    Dictionary<int, ScoreboardCard> scorecards = new Dictionary<int, ScoreboardCard>();

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
        scoreboard.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            scoreboard.SetActive(true);
        if (Input.GetKeyUp(KeyCode.Tab))
            scoreboard.SetActive(false);
    }

    public static void PlayerJoined(int clientID)
    {
        ScoreboardCard newCard = Instantiate(instance.cardPrefab, instance.cardParent);
        instance.scorecards.Add(clientID, newCard);
        if(PlayerManager.instance._players.TryGetValue(clientID, out PlayerManager.Player player))
        {
            newCard.Initialzie(player.playerName);
        }
        else
        {
            Debug.Log("Had a problem with getting the correct playername");
        }
    }

    public static void PlayerLeft(int clientID)
    {
        if(instance.scorecards.TryGetValue(clientID, out ScoreboardCard scorecard))
        {
            Destroy(scorecard.gameObject);
            instance.scorecards.Remove(clientID);
        }
    }

    public static void SetHealthText(string health)
    {
        instance.healthText.text = health;
    }

    //setting kill count methods
    public static void SetKills(int clientID, int kills)
    {
        instance.SetKillsServer(clientID, kills);
    }
    [Server]
    public void SetKillsServer(int clientID, int kills)
    {
        SetKillsObserver(clientID, kills);
    }
    [ObserversRpc]
    private void SetKillsObserver(int clientID, int kills)
    {
        instance.scorecards[clientID].SetKills(kills);
    }

    //setting death count methods
    public static void SetDeaths(int clientID, int deaths)
    {
        instance.SetDeathsServer(clientID, deaths);
    }
    [Server]
    public void SetDeathsServer(int clientID, int deaths)
    {
        SetDeathsObserver(clientID, deaths);
    }
    [ObserversRpc]
    private void SetDeathsObserver(int clientID, int deaths)
    {
        instance.scorecards[clientID].SetDeaths(deaths);
    }

    //Sets the individual player's name on their screen
    [TargetRpc]
    private void SetPlayerNameTarget(NetworkConnection conn, string name)
    {

    }
}
