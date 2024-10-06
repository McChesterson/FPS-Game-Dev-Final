using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;

public class PlayerController : NetworkBehaviour
{
    public static Dictionary<int, PlayerController> Players = new Dictionary<int, PlayerController>();
    public PlayerWepon _playerWeapon;

    [Header("Base Setup")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8f;
    public float gravity = 20f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    [SerializeField] private int playerSelfLayer = 7;

    CharacterController characterController;
    Vector3 _moveDirection = Vector3.zero;
    float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;

    [SerializeField]
    private float cameraYOffset = 0.4f;
    private Camera playerCamera;

    [Header("Animator Setup")]
    public Animator anim;

    [SerializeField] GameObject visorObject;
    public override void OnStartClient()
    {
        base.OnStartClient();

        Players.Add(OwnerId, this);
        PlayerManager.InitializeNewPlayer(OwnerId);
        GameUIManager.PlayerJoined(OwnerId);

        if (base.IsOwner)
        {

            visorObject.SetActive(false);
            playerCamera = Camera.main;
            playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, transform.position.z);
            playerCamera.transform.SetParent(transform);

            if (TryGetComponent(out PlayerWepon playerWepon))
            {
                playerWepon.InitializeWeapons(playerCamera.transform);
                _playerWeapon = playerWepon;
            }

            gameObject.layer = playerSelfLayer;
            foreach (Transform child in transform)
                child.gameObject.layer = playerSelfLayer;
        }
        else
        {
            gameObject.GetComponent<PlayerController>().enabled = false;
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        Players.Remove(OwnerId);
        PlayerManager.PlayerDisconnected(OwnerId);
        GameUIManager.PlayerLeft(OwnerId);
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //rb = GetComponent<Rigidbody>();
        //rb.freezeRotation = true;

        //readyToJump = true;
    }

    void Update()
    {
        if (!canMove) return;

        //player movement code
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        Vector2 input;
        input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        input.Normalize();

        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * input.y : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * input.x : 0;

        float movementDirectionY = _moveDirection.y;
        _moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            _moveDirection.y = jumpSpeed;
        }
        else
        {
            _moveDirection.y = movementDirectionY;
        }
        if (!characterController.isGrounded)
        {
            _moveDirection.y -= gravity * Time.deltaTime;
        }

        //move the controller
        characterController.Move(_moveDirection * Time.deltaTime);

        //player and camera rotation
        if (canMove && playerCamera != null)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    //network methods
    public static void SetPlayerPosition(int clientID, Vector3 position)
    {
        if (!Players.TryGetValue(clientID, out PlayerController player))
            return;

        //empties the players inventory
        //this code is called here becuase SetPlayerPosition is only called on a respawn
        //(it's jank, but it works)
        //PS: it doesn't work
        //player._playerWeapon.ResetInventory();

        player.SetPlayerPositionServer(position);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerPositionServer(Vector3 position)
    {
        SetPlayerPositionTarget(Owner, position);
    }
    [TargetRpc]
    private void SetPlayerPositionTarget(NetworkConnection conn, Vector3 position)
    {
        transform.position = position;
    }

    public static void TogglePlayer(int clientID, bool toggle)
    {
        if (!Players.TryGetValue(clientID, out PlayerController player))
            return;

        player.TogglePlayerServer(toggle);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TogglePlayerServer(bool toggle)
    {
        DisablePlayerObserver(toggle);
    }

    [ObserversRpc]
    private void DisablePlayerObserver(bool toggle)
    {
        canMove = toggle;

        if (TryGetComponent(out Renderer playerRenderer))
            playerRenderer.enabled = toggle;
        if (TryGetComponent(out CapsuleCollider collider))
            collider.enabled = toggle;
        characterController.enabled = toggle;

        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        foreach(var rend in allRenderers)
        {
            rend.enabled = toggle;
        }
    }
}
