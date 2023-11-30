using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nope : NetworkBehaviour
{
    public GameObject playerPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(base.Owner);
        Vector3 position;
        Quaternion rotation;
        NetworkManager _networkManager = InstanceFinder.NetworkManager;
        _networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;

        
        foreach (KeyValuePair<int, NetworkConnection> kvp in _networkManager.ClientManager.Clients)
        {
            Debug.Log(kvp.Key + kvp.Value.ToString());
            GameObject go = Instantiate(playerPrefab);

            InstanceFinder.ServerManager.Spawn(go, kvp.Value);

            //Console.WriteLine("Key: " + kvp.Key + ", Value: " + kvp.Value);
        }

        //NetworkObject nob = _networkManager.GetPooledInstantiated(playerPrefab, position, rotation, true);
        //_networkManager.ServerManager.Spawn(nob, conn);
        //GameObject go = Instantiate(playerPrefab);

        //InstanceFinder.ServerManager.Spawn(go, base.Owner);
        //Debug.Log("spawned");
    }

    private void SceneManager_OnClientLoadedStartScenes(NetworkConnection arg1, bool arg2)
    {
        Debug.Log("hullo spawned a thing");
        InstanceFinder.ServerManager.Spawn(playerPrefab,arg1);
    }
}
