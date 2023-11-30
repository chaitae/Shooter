using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class PlayerControllerNet : NetworkBehaviour
{
    [SerializeField]
    private CharacterController characterController;
    [SerializeField]
    private float defaultSpeed = 2f;
    [SerializeField]
    private float sprintSpeed = 4f;
    private bool groundedPlayer;
    [SerializeField]
    private float jumpHeight = 1.0f;
    [SerializeField] private float gravityValue = -9.81f;
    private Vector3 playerVelocity;
    public Action<bool> onMove;
    public Action onJump;
    int damage = 1;
    Vector3 move;
    GameObject vCamGO;
    CinemachineVirtualCamera vCam;
    float currSpeed;
    //move this form onstart to start game
    public override void OnStartNetwork()
    {
        if (!base.Owner.IsLocalClient)
        {
            gameObject.GetComponent<PlayerControllerNet>().enabled = false;
        }
        else
        {
            characterController = GetComponent<CharacterController>();
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            vCamGO = GameObject.Find("CMvcam");
            vCam = vCamGO.GetComponent<CinemachineVirtualCamera>();
            vCam.Follow = this.gameObject.transform;
            vCam.LookAt = this.gameObject.transform;
        }
    }
    private void Update()
    {
        //do intput
        if (!base.IsOwner) return;
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
            health.OnDamage(damageToGive, OnTerminatedOpponent);
        }
        Debug.DrawRay(position, direction, Color.green);

    }
    // Update is called once per frame
    void FixedUpdate()
    {
        playerVelocity.y += gravityValue * Time.deltaTime;
        characterController.Move(move * Time.deltaTime * currSpeed);
        characterController.Move(playerVelocity * Time.deltaTime);


    }
}
