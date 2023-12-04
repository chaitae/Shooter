using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using FishNet.Example.ColliderRollbacks;

public class PlayerControllerNet : NetworkBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float defaultSpeed = 2f;
    [SerializeField] private float sprintSpeed = 4f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float gravityValue = -9.81f;

    private bool groundedPlayer;
    private Vector3 playerVelocity;
    private bool canMove = true;

    public Action<bool> onMove;
    public Action onJump;
    public Action<bool> onShoot;
    public Action OnKilledOpponent;

    int damage = 1;
    Vector3 move;
    GameObject vCamGO;
    private bool isRoundActive = false;
    CinemachineVirtualCamera vCam;
    private float currSpeed;
    Health health;
    public GameObject visualEntity;
    private float reloadTime = 2f;
    private int maxAmmo = 100;
    public GameObject straw;


    public override void OnStartNetwork()
    {
        if (!base.Owner.IsLocalClient)
        {
            gameObject.GetComponent<PlayerControllerNet>().enabled = false;
        }
        else
        {
            health = GetComponent<Health>();
            characterController = GetComponent<CharacterController>();
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            health.OnDeath += OnDeath;
            health.OnRevive += OnRespawn;
            vCamGO = GameObject.Find("CMvcam");
            if (vCamGO != null )
            {
                SetUpPlayer();
            }
            GameManager.OnEndMatch += OnEndMatch;
            GameManager.OnStartMatch += SetUpPlayer;
        }

    }


    private void OnDisable()
    {
        GameManager.OnEndMatch -= OnEndMatch;

    }
    [ObserversRpc]
    private void OnEndMatch()
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
        gameObject.transform.position = PlayerManager.instance.spawnLocations[randLocationIndex].transform.position;
        canMove = true;
    }

    private void OnDeath()
    {
        canMove = false;
    }

    //This is for Steam lobby puroses
    private void SetUpPlayer()
    {
        canMove = true;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        isRoundActive = true;
        vCam = vCamGO.GetComponent<CinemachineVirtualCamera>();
        vCam.Follow = this.gameObject.transform;
        vCam.LookAt = this.gameObject.transform;
        if (vCam != null) vCam.enabled = true;
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

        // Changes the height position of the player..
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }
        if (Input.GetButton("Fire1"))
        {
            if (PlayerManager.instance.players[base.OwnerId].bullets > 0)
            {
                Shoot();
                onShoot?.Invoke(true);
            }
            else
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
    void FixedUpdate()
    {
        if (!base.IsOwner || !canMove) return;
        playerVelocity.y += gravityValue * Time.deltaTime;
        characterController.Move(move * Time.deltaTime * currSpeed);
        characterController.Move(playerVelocity * Time.deltaTime);


    }
    private void Shoot()
    {
        ShootServer(damage, vCamGO.transform.position, vCamGO.transform.forward);
    }

    [ServerRpc(RequireOwnership = false)]
    private void Reload()
    {
        if(!PlayerManager.instance.players[base.OwnerId].isReloading)
        StartCoroutine(ReloadCoroutine());
    }
    private IEnumerator ReloadCoroutine()
    {
        // Get the player from the PlayerManager
        Player currentPlayer = PlayerManager.instance.players[base.OwnerId];

        // Set isReloading to true
        currentPlayer.isReloading = true;
        PlayerManager.instance.players[base.OwnerId] = currentPlayer;
        PlayerManager.instance.players.Dirty(base.OwnerId);

        // Log reloading message
        Debug.Log("Reloading.........");

        // Wait for the reload time
        yield return new WaitForSeconds(reloadTime);

        // Set isReloading to false
        currentPlayer.isReloading = false;

        PlayerManager.instance.players[base.OwnerId] = currentPlayer;
        PlayerManager.instance.players.Dirty(base.OwnerId);

        Debug.Log("Reload complete!");

        // Reset bullets to max ammo
        currentPlayer.bullets = maxAmmo;

        PlayerManager.instance.players[base.OwnerId] = currentPlayer;
        PlayerManager.instance.players.Dirty(base.OwnerId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void ShootServer(int damageToGive, Vector3 position, Vector3 direction)
    {
        Player tempPlayer = PlayerManager.instance.players[base.OwnerId];
        tempPlayer.bullets--;
        PlayerManager.instance.players[base.OwnerId] = tempPlayer;
        PlayerManager.instance.players.Dirty(base.OwnerId);
        if (Physics.Raycast(position, direction, out RaycastHit hit) && hit.transform.TryGetComponent(out Health health))
        {
            health.OnDamage(damageToGive, OnKilledOpponent,base.OwnerId);
        }
        Debug.DrawRay(position, direction, Color.green);

    }
}
