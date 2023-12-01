using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
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
    public static Action<NetworkConnection> OnAddPlayer;
    public static Action OnLeaderBoardDataChanged;
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
    public void TestDefaultPlayerValues(List<Player> _players)
    {
        Debug.Log(players == null);
        Debug.Log(_players == null);
        Debug.Log(_players.Count);
        for(int i =0; i<_players.Count; i++)
        {
            Debug.Log(_players[i]);
            players.Add(_players[i]);
            players.Dirty(0);
        }

    }
    [Server]
    public void UpdateKillRecords(int victim, int slayer)
    {
        Debug.Log("inside update kill records");
        Player pSlayer = players[slayer];
        Player pVictim = players[victim];
        //check if victims and slayers preset
        if(pSlayer.victims == null)
        {
            pSlayer.victims = new Dictionary<string,int>();
        }
        if(pVictim.slayers == null)
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
        if(pVictim.slayers.ContainsKey(pSlayer.steamName))
        {
            pVictim.slayers[pSlayer.steamName]++;
        }
        else
        {
            pVictim.slayers.Add(pSlayer.steamName, 1);
        }
        //Player omg = new Player { clientID = 0, steamName = "nooo", slayers = new Dictionary<Player, int>(), victims = new Dictionary<Player, int>() };
        //omg.slayers.Add(omg, 1);
        //players[slayer] = omg;
        players[victim] = pVictim;
        players[slayer] = pSlayer;
        //setting the values seems to break this
        //i wonder if we set it to not the existing i could fix it?
        //i think it might be because i'm doing a player inside a type player
        //okay so it looks like you want to make a new player and fill the values
        //yep yep so it's because it is using players you shouldn't do that lets do an int now..
        Debug.Log("victim:" + victim + " slayer:" + slayer);
        OnLeaderBoardDataChanged?.Invoke();
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
        Debug.Log(networkConnection.ClientId +"on client loadedStartScenes");
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
        players.Dirty(0);
        OnAddPlayer?.Invoke(networkConnection);
    }
}
