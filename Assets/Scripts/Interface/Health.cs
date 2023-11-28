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
            gameObject.SetActive(false);
        }
    }
    public void OnDamage(int damage,Action onKill)
    {
        if (health <= 0) return;
        Debug.Log(health);
        health -= damage;
        if(health <= 0)
        {
            onKill?.Invoke();
            gameObject.SetActive(false);
        }
    }
}
