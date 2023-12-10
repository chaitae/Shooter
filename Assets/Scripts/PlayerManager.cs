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
    public int lives;
    public int bullets;
    public bool isReloading;
    public NetworkConnection networkCOnnection;
}
public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager instance;
    private NetworkManager _networkManager;

    [SyncObject]
    public readonly SyncList<Player> players = new SyncList<Player>();

    public GameObject playerPrefab;
    public static int defaultLivesCount = 3;
    int defaultBulletCount = 10;

    public static Action<NetworkConnection> OnAddPlayer;
    public static Action OnLeaderBoardDataChanged;

    public List<GameObject> spawnLocations = new List<GameObject>();
    private List<GameObject> availableSpawns;

    public PlayerControllerNet localPlayerController;
    //todo: maybe disable vcam initially
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            availableSpawns = new List<GameObject>(spawnLocations);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnEnable()
    {
        GameManager.OnStartMatch += ResetPlayers;
    }
    private void OnDisable()
    {
        GameManager.OnStartMatch-= ResetPlayers;
    }
    [Server]
    public void ResetPlayers()
    {
        Debug.Log("reset players..");
        for(int i =0; i<players.Count; i++)
        {
            players[i] = ReturnResetPlayer(players[i].steamName, players[i].clientID);
        }
        players.DirtyAll();
    }
    Player ReturnResetPlayer(string _name,int _clientID)
    {
        return new Player
        {
            clientID = _clientID,
            steamName = _name,
            lives = 3,
            isReloading = false,
            bullets = defaultBulletCount,
            slayers = new Dictionary<string, int>(),
            victims = new Dictionary<string, int>(),
        };
    }

    [Server]
    public void UpdateKillRecords(int victim, int slayer)
    {
        Player pSlayer = players[slayer];
        Player pVictim = players[victim];
        // Ensure victims and slayers dictionaries are initialized
        pSlayer.victims ??= new Dictionary<string, int>();
        pVictim.slayers ??= new Dictionary<string, int>();

        pVictim.lives--;
        // Update slayer's victims count
        UpdateDictionaryCount(pSlayer.victims, pVictim.steamName);

        // Update victim's slayers count
        UpdateDictionaryCount(pVictim.slayers, pSlayer.steamName);

        players[victim] = pVictim;
        players[slayer] = pSlayer;
        players.Dirty(victim);
        players.Dirty(slayer);
        int livePlayerCount = players.Where((item, index) => (item.lives > 0) ).Count();
        if(livePlayerCount <= 1)
        {
            //End the round
            GameManager.instance.RPCEndMatch();
        }
        OnLeaderBoardDataChanged?.Invoke();
    }
    private void UpdateDictionaryCount(Dictionary<string, int> dictionary, string key)
    {
        if (dictionary.ContainsKey(key))
        {
            dictionary[key]++;
        }
        else
        {
            dictionary.Add(key, 1);
        }
    }
    public GameObject GetRandomSpawnLocation()
    {
        if (availableSpawns.Count == 0)
        {
            Debug.Log("No available spawn locations left.");
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0, availableSpawns.Count);
        GameObject selectedSpawn = availableSpawns[randomIndex];
        availableSpawns.RemoveAt(randomIndex);
        return selectedSpawn;
    }
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        _networkManager = InstanceFinder.NetworkManager;
        _networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
    }

    void CreatePlayer(NetworkConnection networkConnection)
    {
        NetworkObject networkOb = _networkManager.GetPooledInstantiated(playerPrefab, playerPrefab.transform.position, playerPrefab.transform.rotation, true);
        _networkManager.ServerManager.Spawn(networkOb, networkConnection);
        networkOb.gameObject.name = networkOb.gameObject.name + networkConnection;
        //choose random presetspawnlocation for gameobject
        networkOb.gameObject.transform.position = GetRandomSpawnLocation().transform.position;
        _networkManager.SceneManager.AddOwnerToDefaultScene(networkOb);
        networkOb.GetComponent<Health>().ownerID = networkConnection.ClientId;
    }
    private void SceneManager_OnClientLoadedStartScenes(NetworkConnection networkConnection, bool asServer)
    {
        if (!asServer)
            return;
        if(!SteamAPI.Init())
        CreatePlayer(networkConnection);

        Player tempPlayer = new Player
        {
            clientID = networkConnection.ClientId,
            lives = GameManager.initialLivesCount,
            slayers = new Dictionary<string, int>(),
            victims = new Dictionary<string, int>(),
            bullets = defaultBulletCount,
            isReloading = false,
            networkCOnnection = networkConnection,
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
