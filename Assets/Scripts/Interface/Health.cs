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
    [SerializeField]int health = 1;
    public Action OnDeath;
    public Action OnRevive;
    [SerializeField]GameObject visualEntity;
    readonly float deathTime = 4f;
    public int ownerID;
    Collider collider;
    private void OnEnable()
    {
        collider = GetComponent<Collider>();
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

    private void OnHealthChange(int prev, int next, bool asServer)
    {
        health = next;

        if (health == 0 && prev != 0)
        {
            Die();
        }
    }
    public void OnDamage(int damage,Action onKill,int attackerID)
    {
        if (!base.IsServer) return;
        if (health == 0) return;
        if(base.OwnerId != -1)//so that this is only ran once
        {
            health = Mathf.Clamp(health-damage, 0, 20);
            if (health == 0)
            {
                Debug.Log("OnDamage called");
                //disable collider
                onKill?.Invoke();
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

        if (PlayerManager.instance.players[ownerID].lives > 0)
        {
            StartCoroutine("TimerSpawn");
        }

    }
    IEnumerator TimerSpawn()
    {
        yield return new WaitForSeconds(deathTime);
        OnRevive?.Invoke();
        visualEntity.SetActive(true); 
        health = 1;
    }
}
