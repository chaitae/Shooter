using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health:NetworkBehaviour
{
    [SyncVar(Channel = Channel.Unreliable, OnChange = nameof(OnHealthChange))]
    [SerializeField]int health = 20;

    private void OnHealthChange(int prev, int next, bool asServer)
    {
        health = next;

        if (health <= 0)
        {
            Die();
            //PlayerManager.instance.
            //death tell player is dead
        }
    }
    public void OnDamage(int damage,Action onKill,int attackerID)
    {
        if (health <= 0) return;
        health -= damage;
        if(health <= 0)
        {
            //PlayerManager.instance.UpdateKillRecords()
            onKill?.Invoke();
            Debug.Log("OwnerID: "+base.OwnerId);
            if(base.OwnerId != -1)
            {
                PlayerManager.instance.UpdateKillRecords(base.OwnerId, attackerID);
            }
            //gameObject.SetActive(false);
        }
    }
    [ObserversRpc]
    public void Die()
    {
        gameObject.SetActive(false);

    }
}
