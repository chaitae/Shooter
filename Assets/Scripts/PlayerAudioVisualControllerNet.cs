using FishNet.Component.Animating;
using FishNet.Example.Scened;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioVisualControllerNet : NetworkBehaviour
{
    private Animator animator,strawAnimator;
    PlayerControllerNet playercontroller;
    private NetworkAnimator networkAnimator;
    public AudioClip shootingSound;
    public AudioClip reloadingSound;
    public AudioSource audioSource;
  
    public override void OnStartNetwork()
    {
        animator = GetComponentInChildren<Animator>();
        playercontroller = GetComponent<PlayerControllerNet>();
        networkAnimator = GetComponent<NetworkAnimator>();
        //strawAnimator.gameObject.SetActive(true);

        strawAnimator = GameObject.Find("CMvcam").GetComponentInChildren<Animator>();
        playercontroller.onMove += SetMoving;
        playercontroller.onJump += Jump;
        playercontroller.onShoot += SetShootStatus;
        playercontroller.onReload += SetReload;
        if(audioSource == null)
        {
            this.enabled = false;
        }
    }

    private void SetReload(bool isReloading)
    {
        strawAnimator.SetBool("isReloading", isReloading);
        if (audioSource != null)
            audioSource.PlayOneShot(reloadingSound);
    }

    private void SetShootStatus(bool isShooting)
    {
        strawAnimator.SetBool("isShooting",isShooting);
        if(audioSource!=null)
        audioSource.PlayOneShot(shootingSound);
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
