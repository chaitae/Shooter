using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
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

    int damage = 1;
    Vector3 move;
    GameObject vCamGO;
    CinemachineVirtualCamera vCam;
    private float currSpeed;
    private float deathTime;
    Health health;
    public GameObject visualEntity;

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
                BootstrapManager.OnStartGame += SetUpPlayer;

            }
        }

    }

    private void OnRespawn()
    {

        //https://discussions.unity.com/t/teleporting-character-issue-with-transform-position-in-unity-2018-3/221631
        //Can enable auto sync transforms in physics settings
        int randLocationIndex = UnityEngine.Random.Range(0, PlayerManager.instance.spawnLocations.Count);
        gameObject.transform.position = PlayerManager.instance.spawnLocations[randLocationIndex].transform.position;
        canMove = true;

        Debug.Log("changed position");
    }

    private void OnDeath()
    {
        canMove = false;
    }



    private void OnDisable()
    {
        BootstrapManager.OnStartGame -= SetUpPlayer;

    }

    private void SetUpPlayer()
    {
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
            Shoot();
        }
    }
    private void Shoot()
    {
        ShootServer(damage, vCamGO.transform.position, vCamGO.transform.forward);
    }
    void OnTerminatedOpponent()
    {

    }

    [ServerRpc(RequireOwnership = false)]
    private void ShootServer(int damageToGive, Vector3 position, Vector3 direction)
    {
        if (Physics.Raycast(position, direction, out RaycastHit hit) && hit.transform.TryGetComponent(out Health health))
        {
            health.OnDamage(damageToGive, OnTerminatedOpponent,base.OwnerId);
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
