using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct Player
{
    public int clientID;
    public string steamName;
    public Dictionary<string, int> slayers;
    public Dictionary<string, int> victims;
}
public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager instance;
    private NetworkManager _networkManager;
    [SyncObject]
    public readonly SyncList<Player> players = new SyncList<Player>();
    [SyncObject]
    public readonly SyncList<int> numbahs = new SyncList<int>();
    [SyncObject]
    public readonly SyncList<string> playerNames = new SyncList<string>();
    public static Action<NetworkConnection> OnAddPlayer;
    public static Action OnLeaderBoardDataChanged;
    public GameObject playerPrefab;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    [Server]
    public void UpdateKillRecords(int victim, int slayer)
    {
        Debug.Log("inside update kill records");
        Player pSlayer = players[slayer];
        Player pVictim = players[victim];
        //check if victims and slayers preset
        if (pSlayer.victims == null)
        {
            pSlayer.victims = new Dictionary<string, int>();
        }
        if (pVictim.slayers == null)
        {
            pVictim.slayers = new Dictionary<string, int>();
        }
        if (pSlayer.victims.ContainsKey(pVictim.steamName))
        {
            pSlayer.victims[pVictim.steamName]++;
        }
        else
        {
            pSlayer.victims.Add(pVictim.steamName, 1);
        }
        if (pVictim.slayers.ContainsKey(pSlayer.steamName))
        {
            pVictim.slayers[pSlayer.steamName]++;
        }
        else
        {
            pVictim.slayers.Add(pSlayer.steamName, 1);
        }
        players[victim] = pVictim;
        players[slayer] = pSlayer;
        players.Dirty(victim);
        players.Dirty(slayer);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log(players.Count);

        }
    }
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        _networkManager = InstanceFinder.NetworkManager;
        _networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
    }
    private void SceneManager_OnClientLoadedStartScenes(NetworkConnection networkConnection, bool asServer)
    {
        if (!asServer)
            return;

        NetworkObject networkOb = _networkManager.GetPooledInstantiated(playerPrefab, playerPrefab.transform.position, playerPrefab.transform.rotation, true);
        _networkManager.ServerManager.Spawn(networkOb, networkConnection);
        _networkManager.SceneManager.AddOwnerToDefaultScene(networkOb);

        Player tempPlayer = new Player
        {
            clientID = networkConnection.ClientId,
        };
        if (SteamAPI.Init())
        {

            int lobbyMemberCount = SteamMatchmaking.GetNumLobbyMembers(new CSteamID(BootstrapManager.CurrentLobbyID));
            CSteamID tempSteamID = (CSteamID)SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(BootstrapManager.CurrentLobbyID), lobbyMemberCount - 1);
            tempPlayer.steamName = SteamFriends.GetFriendPersonaName(tempSteamID);
        }
        else
        {
            tempPlayer.steamName = networkConnection.ClientId.ToString();
        }

        players.Add(tempPlayer);
    }
}
