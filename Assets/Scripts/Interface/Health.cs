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
        UpdateHealth(health);
        if (health == 0)
        {
            Die();
            onKill?.Invoke();
            PlayerManager.instance.UpdateKillRecords(base.OwnerId, attackerID);
        }
    }
    [ObserversRpc]
    public void UpdateHealth(int _health)
    {
        health = _health;
    }
    [ObserversRpc]
    public void Die()
    {
        OnDeath?.Invoke();
        if (PlayerManager.instance.players[ownerID].lives > 0)
        {
            if(!inTimerSpawn)
            StartCoroutine("TimerSpawn");
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
