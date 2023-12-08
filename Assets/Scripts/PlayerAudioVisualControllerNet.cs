using FishNet.Component.Animating;
using FishNet.Example.Scened;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioVisualControllerNet : NetworkBehaviour
{
    public Animator playerAnimator;
    private Animator strawAnimator;
    PlayerControllerNet playercontroller;
    private NetworkAnimator networkAnimator;
    public AudioClip shootingSound;
    public AudioClip reloadingSound;
    public AudioSource audioSource;
    public GameObject fpsStraw;
    public ParticleSystem thirdPersonParticleSystem;
  
    public override void OnStartNetwork()
    {
        //animator = GetComponentInChildren<Animator>();
        playercontroller = GetComponent<PlayerControllerNet>();
        networkAnimator = GetComponent<NetworkAnimator>();
        strawAnimator = GameObject.Find("CMvcam").GetComponentInChildren<Animator>();
        playercontroller.onMove += SetMoving;
        playercontroller.onJump += Jump;
        playercontroller.onShoot += SetShootStatus;
        playercontroller.onReload += SetReload;
        playercontroller.health.OnDeath += Dead;
        playercontroller.health.OnRevive += Revive;
        playercontroller.onShoot += ShowShootServer;
        GameManager.OnStartMatch += OnStartRound;
        if (audioSource == null)
        {
            this.enabled = false;
        }
        if (base.Owner.IsLocalClient)
        {
            fpsStraw.SetActive(false);
        }

    }
    [ServerRpc(RequireOwnership =false)]
    void ShowShootServer(bool isShooting)
    {
        Shoot(isShooting);
    }
    [ObserversRpc]
    private void Shoot(bool isShooting)
    {
        if (isShooting) 
        {
            thirdPersonParticleSystem.gameObject.SetActive(true);
        }
        else
        {

            thirdPersonParticleSystem.gameObject.SetActive(false);
        }
        //throw new NotImplementedException();
    }

    private void Revive()
    {
        playerAnimator.SetBool("isDead", false);
    }

    private void OnStartRound()
    {
        playerAnimator.SetBool("isRunning", false);
        playerAnimator.SetBool("isDead", false);
    }

    private void Dead()
    {
        playerAnimator.SetBool("isDead", true);
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
        playerAnimator.SetBool("isRunning", value);
        //animator.SetBool("isMoving", value);
    }
    public void Jump()
    {
        networkAnimator.SetTrigger("jump");
    }
}
