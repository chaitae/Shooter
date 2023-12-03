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
    public Action OnKilledOpponent;
    int currentAmmo = 20;

    int damage = 1;
    Vector3 move;
    GameObject vCamGO;
    CinemachineVirtualCamera vCam;
    private float currSpeed;
    Health health;
    public GameObject visualEntity;
    private float reloadTime = 2f;
    private int maxAmmo = 100;


    //move this form onstart to start game
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
                vCam = vCamGO.GetComponent<CinemachineVirtualCamera>();
                vCam.Follow = this.gameObject.transform;
                vCam.LookAt = this.gameObject.transform;

            }
            else
            {
                //BootstrapManager.OnStartGame += SetUpPlayer;
                GameManager.OnEndMatch += OnEndMatch;
                GameManager.OnStartMatch += SetUpPlayer;

            }
        }

    }


    private void OnDisable()
    {
        BootstrapManager.OnStartGame -= SetUpPlayer;
        GameManager.OnEndMatch -= OnEndMatch;

    }
    [ObserversRpc]
    private void OnEndMatch()
    {
        Debug.Log("unlock mouse");
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        vCam.enabled = false;
        //throw new NotImplementedException();
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
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        vCam = vCamGO.GetComponent<CinemachineVirtualCamera>();
        vCam.Follow = this.gameObject.transform;
        vCam.LookAt = this.gameObject.transform;
    }

    private void Update()
    {
        if (!base.IsOwner) return;
        if (!canMove) return;
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
            //save initial position?
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }
        if (Input.GetButton("Fire1"))
        {
            //Debug.Log(PlayerManager.instance.players[base.OwnerId].bullets + "before bullets");
            if (PlayerManager.instance.players[base.OwnerId].bullets > 0)
            {

                Debug.Log("bang");
                Shoot();
            }
            else
            {
                Debug.Log("noshoot");

                Reload();
            }
        }
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
    // Update is called once per frame
    void FixedUpdate()
    {
        if (!base.IsOwner) return;
        if (!canMove) return;
        playerVelocity.y += gravityValue * Time.deltaTime;
        characterController.Move(move * Time.deltaTime * currSpeed);
        characterController.Move(playerVelocity * Time.deltaTime);


    }
}
