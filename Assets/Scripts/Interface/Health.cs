using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health:NetworkBehaviour
{
    //Todo: Need to create a sub playerHealth that inherits from Health and gut this function
    int health = 1;
    public Action OnDeath;
    public Action OnRevive;
    [SerializeField]GameObject visualEntity;
    readonly float deathTime = 4f;
    public int ownerID;
    bool inTimerSpawn = false;

    private void OnEnable()
    {
        GameManager.OnStartMatch += ResetHealth;
    }
    private void OnDisable()
    {

        GameManager.OnStartMatch -= ResetHealth;
    }
    private void ResetHealth()
    {
        health = 1;
    }

    public void OnDamage(int damage,Action onKill,int attackerID)
    {
        if (!base.IsServer) return;
        if (health == 0) return;
        health = Mathf.Clamp((health - damage), 0, 20);
        UpdateHealthRPC(health);
        if (health == 0)
        {
            DieObserverRPC();
            onKill?.Invoke();
            PlayerManager.instance.UpdateKillRecordsRPC(base.OwnerId, attackerID);
        }
    }
    [ObserversRpc]
    public void UpdateHealthRPC(int _health)
    {
        health = _health;
    }
    [ServerRpc(RequireOwnership = true)]
    public void DieRPC()
    {
        DieObserverRPC();
    }
    [ObserversRpc]
    public void DieObserverRPC()
    {
        OnDeath?.Invoke();
        if (PlayerManager.instance.players[PlayerManager.instance.GetPlayerMatchingIDIndex(base.OwnerId)].lives > 0)
        {
            Debug.Log("died");
            if(!inTimerSpawn)
            StartCoroutine(TimerSpawn());
        }

    }
    IEnumerator TimerSpawn()
    {
        inTimerSpawn = true;
        yield return new WaitForSeconds(deathTime);
        OnRevive?.Invoke();
        //visualEntity.SetActive(true); 
        health = 1;
        inTimerSpawn = false; ;
    }
}
