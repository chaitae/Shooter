using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using FishNet.Example.ColliderRollbacks;
using FishNet.Connection;
using UnityEngine.Android;
using System.Linq;

public class PlayerControllerNet : NetworkBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private SkinnedMeshRenderer playerModel;
    [SerializeField] private float defaultSpeed = 6f;
    [SerializeField] private float sprintSpeed =8f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float gravityValue = -9.81f;

    // Movement-related variables
    private bool groundedPlayer;
    private Vector3 playerVelocity;
    private bool canMove = true;
    private bool isCrouching = false;
    private Vector3 move;

    // Combat-related variables
    private int damage = 1;
    private float currSpeed;
    private bool fireReady = true;
    private float reloadTime = 2f;
    private int maxAmmo = 10;
    private float fireRate = 0.1f; // Adjust this to control the rate of fire
    private float nextFireTime;

    // Actions
    public Action<bool> onMove;
    public Action onJump;
    public Action<bool> onShoot;
    public Action<bool> onReload;
    public Action OnKilledOpponent;
    public Action<bool> onPaused;

    // Components and Game Objects
    public Health health;
    public GameObject visualEntity;
    public GameObject vCamGO;
    public GameObject straw;
    public GameObject followTarget;
    public GameObject firstPersonStraw;

    // Cinemachine
    private CinemachineVirtualCamera vCam;

    // Round-related variables
    private bool isRoundActive = false;

    private Coroutine heightAdjustmentCoroutine;
    private bool paused;


    public override void OnStartNetwork()
    {
        if (!base.Owner.IsLocalClient)
        {
            gameObject.GetComponent<PlayerControllerNet>().enabled = false;
        }
        else
        {
            PlayerManager.instance.localPlayerController = this;
            playerModel.enabled = false;
            health = GetComponent<Health>();
            characterController = GetComponent<CharacterController>();
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            health.OnDeath += OnDeath;
            health.OnRevive += OnRespawn;
            //todo: instead of finding vcam instantiate it in setupplayer
            vCamGO = GameObject.Find("CMvcam");
            if (vCamGO != null )
            {
                SetUpPlayer();
            }
            GameManager.OnEndMatch += OnEndMatchRPC;
            GameManager.OnStartMatch += SetUpPlayer;
        }

    }


    private void OnDisable()
    {
        GameManager.OnEndMatch -= OnEndMatchHelperObserver;

    }
    [Server]
    void OnEndMatchRPC()
    {
        OnEndMatchHelperObserver();
    }
    /// <summary>
    /// RPC method invoked by observers to handle the end of the match. Disables round activity, unlocks the mouse cursor, and disables the virtual camera.
    /// </summary>
    [ObserversRpc]
    private void OnEndMatchHelperObserver()
    {
        isRoundActive= false;
        Debug.Log("unlock mouse");
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        if(vCam!= null)
        vCam.enabled = false;
    }

    private void OnRespawn()
    {

        //https://discussions.unity.com/t/teleporting-character-issue-with-transform-position-in-unity-2018-3/221631
        //Can enable auto sync transforms in physics settings
        int randLocationIndex = UnityEngine.Random.Range(0, PlayerManager.instance.spawnLocations.Count);
        //gameObject.transform.position = PlayerManager.instance.spawnLocations[randLocationIndex].transform.position; will need to update how spawn locations are
        canMove = true;
    }

    private void OnDeath()
    {
        canMove = false;
    }
    /// <summary>
    /// Sets up the player, instantiates the first-person straw, enables movement, locks the cursor, and activates the round.
    /// </summary>
    private void SetUpPlayer()
    {
        //todo: move set visual for gameobjct to playeraudiovisual controller
        Instantiate(firstPersonStraw, vCamGO.transform);
        canMove = true;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        isRoundActive = true;
        vCam = vCamGO.GetComponent<CinemachineVirtualCamera>();
        vCam.Follow = followTarget.transform;
        vCam.LookAt = followTarget.transform;
        if (vCam != null) vCam.enabled = true;
    }

    private IEnumerator AdjustHeight(float targetHeight, float duration)
    {
        float elapsedTime = 0f;
        float startHeight = characterController.height;

        while (elapsedTime < duration)
        {
            // Interpolate between the start and target heights
            float newHeight = Mathf.Lerp(startHeight, targetHeight, elapsedTime / duration);

            // Set the new height for both CharacterController and capsuleCollider
            characterController.height = newHeight;
            capsuleCollider.height = newHeight;

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Ensure the final height is set
        characterController.height = targetHeight;
        capsuleCollider.height = targetHeight;

        // Reset the coroutine reference
        heightAdjustmentCoroutine = null;
    }

    private void AdjustHeightSmoothly(float targetHeight)
    {
        // If a height adjustment coroutine is already running, stop it
        if (heightAdjustmentCoroutine != null)
        {
            StopCoroutine(heightAdjustmentCoroutine);
        }

        float duration = 0.2f; // Adjust the duration as needed

        // Start the new coroutine
        heightAdjustmentCoroutine = StartCoroutine(AdjustHeight(targetHeight, duration));
    }
    private void Update()
    {
        if (!base.IsOwner || !canMove || !isRoundActive) return;
        groundedPlayer = characterController.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
        currSpeed = Input.GetButton("Sprint") ? sprintSpeed : defaultSpeed;
        gameObject.transform.forward = new Vector3(vCam.transform.forward.x, 0, vCam.transform.forward.z);
        move = characterController.transform.forward * Input.GetAxis("Vertical") + characterController.transform.right*Input.GetAxis("Horizontal");

        onMove?.Invoke(Mathf.Abs(Input.GetAxis("Vertical")) > 0 || Mathf.Abs(Input.GetAxis("Horizontal")) > 0);
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;
            vCam.enabled = paused ? false : true;
            onPaused?.Invoke(paused);
            UnityEngine.Cursor.lockState = paused? CursorLockMode.None: CursorLockMode.Locked;

        }
        if (Input.GetButtonDown("Crouch"))
        {
            isCrouching = !isCrouching;
            float targetHeight = isCrouching ? 1f : 2f;
            AdjustHeightSmoothly(targetHeight);
        }
        // Changes the height position of the player..
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }
        if (Input.GetButton("Fire1"))
        {
            int bulletCount = PlayerManager.instance.players.Where(player => player.clientID == base.OwnerId).ElementAt(0).bullets;
            if (bulletCount > 0 && fireReady)
            {
                Shoot();
                onShoot?.Invoke(true);
                StartCoroutine(StartFireCoolDown());
            }
            else if(bulletCount <= 0)
            {
                onShoot?.Invoke(false);
                Reload();
            }
        }
        else
        {
            onShoot?.Invoke(false);
        }
    }
    IEnumerator StartFireCoolDown()
    {
        fireReady = false;
        yield return new WaitForSeconds(.5f);
        fireReady = true;
    }
    void FixedUpdate()
    {
        if (!base.IsOwner || !canMove) return;
        playerVelocity.y += gravityValue * Time.deltaTime;
        characterController.Move(move * Time.deltaTime * currSpeed);
        characterController.Move(playerVelocity * Time.deltaTime);
    }
    private void Shoot()
    {
        ShootServerRPC(damage, vCamGO.transform.position, vCamGO.transform.forward);
    }

    [ServerRpc(RequireOwnership = false)]
    private void Reload()
    {
        if(!PlayerManager.instance.players[PlayerManager.instance.GetPlayerMatchingIDIndex(base.OwnerId)].isReloading)
        StartCoroutine(ReloadCoroutine());
    }
    /// <summary>
    /// Coroutine for handling the reloading process. Updates player data during reloading and resets bullets to max ammo after completion.
    /// </summary>
    private IEnumerator ReloadCoroutine()
    {
        // Get the player from the PlayerManager
        Player currentPlayer = PlayerManager.instance.players[PlayerManager.instance.GetPlayerMatchingIDIndex(base.OwnerId)];

        // Set isReloading to true
        currentPlayer.isReloading = true;
        PlayerManager.instance.players[PlayerManager.instance.GetPlayerMatchingIDIndex(base.OwnerId)] = currentPlayer;
        PlayerManager.instance.players.Dirty(PlayerManager.instance.players[PlayerManager.instance.GetPlayerMatchingIDIndex(base.OwnerId)]);
        SetLocalIsReloadingRPC(base.ClientManager.Connection, currentPlayer.isReloading);
        // Log reloading message
        Debug.Log("Reloading.........");

        // Wait for the reload time
        yield return new WaitForSeconds(reloadTime);

        // Set isReloading to false
        currentPlayer.isReloading = false;

        PlayerManager.instance.players[PlayerManager.instance.GetPlayerMatchingIDIndex(base.OwnerId)] = currentPlayer;
        PlayerManager.instance.players.Dirty(PlayerManager.instance.players[PlayerManager.instance.GetPlayerMatchingIDIndex(base.OwnerId)]);

        Debug.Log("Reload complete!");

        SetLocalIsReloadingRPC(base.ClientManager.Connection, currentPlayer.isReloading);
        // Reset bullets to max ammo
        currentPlayer.bullets = maxAmmo;

        PlayerManager.instance.players[PlayerManager.instance.GetPlayerMatchingIDIndex(base.OwnerId)] = currentPlayer;
        PlayerManager.instance.players.Dirty(PlayerManager.instance.players[PlayerManager.instance.GetPlayerMatchingIDIndex(base.OwnerId)]);
        Debug.Log(PlayerManager.instance.players[PlayerManager.instance.GetPlayerMatchingIDIndex(base.OwnerId)].bullets);

    }
    [TargetRpc]
    private void SetLocalIsReloadingRPC(NetworkConnection conn, bool isReloading)
    {
        onReload?.Invoke(isReloading);
    }
    /// <summary>
    /// Server RPC method for handling player shooting. Decreases the player's bullet count, updates the player's data, and performs a raycast for dealing damage.
    /// </summary>
    /// <param name="damageToGive">The damage to inflict on the target.</param>
    /// <param name="position">The position from which the shot originates.</param>
    /// <param name="direction">The direction in which the shot is fired.</param>
    [ServerRpc(RequireOwnership = false)]
    private void ShootServerRPC(int damageToGive, Vector3 position, Vector3 direction)
    {
        Player tempPlayer = PlayerManager.instance.players[PlayerManager.instance.GetPlayerMatchingIDIndex(base.OwnerId)];
        tempPlayer.bullets--;
        PlayerManager.instance.players[PlayerManager.instance.GetPlayerMatchingIDIndex(base.OwnerId)] = tempPlayer; // need to make a method that finds player and client id
        PlayerManager.instance.players.Dirty(PlayerManager.instance.GetPlayerMatchingIDIndex(base.OwnerId));
        if (Physics.Raycast(position, direction, out RaycastHit hit) && hit.transform.TryGetComponent(out Health health))
        {
            DebugGUI.LogMessage("dammage called");
            health.OnDamage(damageToGive, OnKilledOpponent,base.OwnerId);
        }
        Debug.DrawRay(position, direction, Color.green);

    }
}
