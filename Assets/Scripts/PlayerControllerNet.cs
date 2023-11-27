using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerNet : NetworkBehaviour
{
    [SerializeField] 
    private CharacterController characterController;
    [SerializeField]
    private float speed = 2f;
    private bool groundedPlayer;
    [SerializeField]
    private float jumpHeight = 1.0f;
    [SerializeField]private float gravityValue = -9.81f;
    private Vector3 playerVelocity;
    public Action<bool> onMove;
    public Action onJump;
    public override void OnStartNetwork()
    {
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!base.IsOwner) return;
        groundedPlayer = characterController.isGrounded;
        //if (groundedPlayer && playerVelocity.y < 0)
        //{
        //    playerVelocity.y = 0f;
        //}
        //Vector3 move = new Vector3(Input.GetAxis("Vertical"), 0, Input.GetAxis("Horizontal")*-1f);

        //bool isMoving = (Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f);
        //onMove?.Invoke(isMoving);
        //if(Input.GetButtonUp("Jump"))
        //{
        //    //onJump?.Invoke();
        //}
        //if (Input.GetButtonDown("Jump") && groundedPlayer)
        //{
        //    playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        //    Debug.Log("hullo");
        //}

        //playerVelocity.y += gravityValue * Time.deltaTime;
        //characterController.Move(move * Time.deltaTime * speed);
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        characterController.Move(move * Time.deltaTime * speed);

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }

        // Changes the height position of the player..
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            //save initial position?
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }
        playerVelocity.y += gravityValue * Time.deltaTime;
        characterController.Move(playerVelocity * Time.deltaTime);


    }
}
