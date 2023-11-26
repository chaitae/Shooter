using FishNet.Component.Animating;
using FishNet.Example.Scened;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationNet : NetworkBehaviour
{
    private Animator animator;
    PlayerControllerNet playercontroller;
    private NetworkAnimator networkAnimator;
    public override void OnStartNetwork()
    {
        animator = GetComponentInChildren<Animator>();
        playercontroller = GetComponent<PlayerControllerNet>();
        networkAnimator = GetComponent<NetworkAnimator>();
        playercontroller.onMove += SetMoving;
        playercontroller.onJump += Jump;
    }
    public override void OnStopServer()
    {
        playercontroller.onMove -= SetMoving;
        playercontroller.onJump -= Jump;
    }
    public void SetMoving(bool value)
    {

        animator.SetBool("isMoving", value);
    }
    public void Jump()
    {
        //animator.SetTrigger("jump");
        networkAnimator.SetTrigger("jump");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
