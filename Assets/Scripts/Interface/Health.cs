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
    [SyncVar(Channel = Channel.Unreliable, OnChange = nameof(OnHealthChange))]
    [SerializeField]int health = 20;
    public Action OnDeath;
    public Action OnRevive;
    [SerializeField]GameObject visualEntity;
    readonly float deathTime = 4f;


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
        //using the base owner id doesn't work because it means it'll only work for that server you'll need to connect the clientid with this health 
        //you need the networkobject

        int ownerID = GetComponent<NetworkObject>().OwnerId; //this is inefficient you'll need to set this on a spawn perhaps
        if (PlayerManager.instance.players[ownerID].lives > 0)
        {
            StartCoroutine("TimerSpawn");
        }
        //if (base.OwnerId != -1)
        //{
        //}

    }
    IEnumerator TimerSpawn()
    {
        yield return new WaitForSeconds(deathTime);
        OnRevive?.Invoke();
        visualEntity.SetActive(true); 
        health = 20;
    }
}
