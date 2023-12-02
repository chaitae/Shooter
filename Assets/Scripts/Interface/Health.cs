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
    public Action OnDeath;
    public Action OnRevive;
    [SerializeField]GameObject visualEntity;
    float deathTime = 4f;


    private void OnHealthChange(int prev, int next, bool asServer)
    {
        health = next;

        if (health <= 0)
        {
            Die();
        }
    }
    public void OnDamage(int damage,Action onKill,int attackerID)
    {
        if (health <= 0) return;
        health -= damage;
        if(health <= 0)
        {
            onKill?.Invoke();
            Debug.Log("OwnerID: "+base.OwnerId);
            if(base.OwnerId != -1)
            {
                PlayerManager.instance.UpdateKillRecords(base.OwnerId, attackerID);
            }
        }
    }
    [ObserversRpc]
    public void Die()
    {
        visualEntity.SetActive(false);
        OnDeath?.Invoke();
        StartCoroutine("TimerSpawn");

    }
    IEnumerator TimerSpawn()
    {
        yield return new WaitForSeconds(deathTime);
        OnRevive?.Invoke();
        visualEntity.SetActive(true);
        health = 20;
    }
}
