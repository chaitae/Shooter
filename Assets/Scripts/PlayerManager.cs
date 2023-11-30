using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Player
{
    public int clientID;
    public string steamName;
    public List<Player> playersDefeated;
    public List<Player> playersBestedMe;
}
public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager instance;
    private NetworkManager _networkManager;
    [SyncObject]
    public readonly SyncList<Player> players = new SyncList<Player>();
    public static Action<NetworkConnection> OnAddPlayer;
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

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        _networkManager = InstanceFinder.NetworkManager;
        _networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
    }
    private void SceneManager_OnClientLoadedStartScenes(NetworkConnection networkConnection, bool asServer)
    {
        //Debug.Log("networkConnection:" + networkConnection.IsHost + "before check as server");
        //if (networkConnection.IsHost)
        //    return;
        if (!asServer)
            return;
        Debug.Log(networkConnection.ClientId +"on client loadedStartScenes");
        Player tempPlayer = new Player
        {
            clientID = networkConnection.ClientId,
            //steamName = SteamFriends.GetFriendPersonaName(tempSteamID)
        };
        if (SteamAPI.Init())
        {

            int lobbyMemberCount = SteamMatchmaking.GetNumLobbyMembers(new CSteamID(BootstrapManager.CurrentLobbyID));
            CSteamID tempSteamID = (CSteamID)SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(BootstrapManager.CurrentLobbyID), lobbyMemberCount - 1);
            tempPlayer.steamName = SteamFriends.GetFriendPersonaName(tempSteamID);
        }
        players.Add(tempPlayer);
        players.Dirty(0);
        OnAddPlayer?.Invoke(networkConnection);
    }
}
