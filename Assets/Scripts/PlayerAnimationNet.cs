using FishNet.Component.Animating;
using FishNet.Example.Scened;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationNet : NetworkBehaviour
{
    private Animator animator,strawAnimator;
    PlayerControllerNet playercontroller;
    private NetworkAnimator networkAnimator;
    public override void OnStartNetwork()
    {
        animator = GetComponentInChildren<Animator>();
        playercontroller = GetComponent<PlayerControllerNet>();
        networkAnimator = GetComponent<NetworkAnimator>();
        strawAnimator = GameObject.Find("CMvcam").GetComponentInChildren<Animator>();
        playercontroller.onMove += SetMoving;
        playercontroller.onJump += Jump;
        playercontroller.onShoot += SetShootStatus;
    }

    private void SetShootStatus(bool isShooting)
    {
        strawAnimator.SetBool("isShooting",isShooting);
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
        networkAnimator.SetTrigger("jump");
    }
}
