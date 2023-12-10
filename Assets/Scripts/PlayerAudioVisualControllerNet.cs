using FishNet.Component.Animating;
using FishNet.Example.Scened;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
    public GameObject spine;
    public GameObject lookatTarget;
    public GameObject root;
    [SerializeField]
    private float rotationOffset;
    private bool soundReady = true;
    private bool isPaused = true;

    public override void OnStartNetwork()
    {
        lookatTarget = GameObject.Find("CMvcam");
        playercontroller = GetComponent<PlayerControllerNet>();
        networkAnimator = GetComponent<NetworkAnimator>();
        strawAnimator = lookatTarget.GetComponentInChildren<Animator>();
        playercontroller.onMove += SetMoving;
        playercontroller.onJump += Jump;
        playercontroller.onShoot += SetShootStatus;
        playercontroller.onReload += SetReload;
        playercontroller.health.OnDeath += Dead;
        playercontroller.health.OnRevive += Revive;
        playercontroller.onShoot += ShowShootServer;
        GameManager.OnStartMatch += OnStartRound;
        playercontroller.onPaused += Pause;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            this.enabled = false;
        }
        if (base.Owner.IsLocalClient)
        {
            fpsStraw.SetActive(false);
        }

    }

    private void Pause(bool _isPaused)
    {
        isPaused = _isPaused;
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
    }
 
    private void Update()
    {
        if (base.IsOwner && !isPaused)
        {
            float mousePercentPos = (Input.mousePosition.y / (float)Screen.height);
            playerAnimator.SetFloat("Aim", mousePercentPos);
        }
    }
    private void Revive()
    {

        playerAnimator.SetLayerWeight(1, 1);
        playerAnimator.SetBool("isDead", false);
    }

    private void OnStartRound()
    {
        playerAnimator.SetLayerWeight(1, 1);
        playerAnimator.SetBool("isRunning", false);
        playerAnimator.SetBool("isDead", false);
    }

    private void Dead()
    {
        playerAnimator.SetLayerWeight(1, 0);
        playerAnimator.SetBool("isDead", true);
    }

    private void SetReload(bool isReloading)
    {
        strawAnimator.SetBool("isReloading", isReloading);
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
        playerAnimator.SetBool("isRunning", value);
    }
    public void Jump()
    {
        networkAnimator.SetTrigger("jump");
    }
}
