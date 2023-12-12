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
/// <summary>
/// Represents a player with essential attributes such as client ID, Steam name, dictionaries for slayers and victims, life count, bullet count, reloading status, and network connection.
/// </summary>
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
/// <summary>
/// Manages players in the game using FishNet for networking. Handles player instantiation, reset, and updates kill records.
/// </summary>
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




    /// <summary>
    /// Called when the network starts. Initializes the network manager and subscribes to the client-loaded event.
    /// </summary>
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        Debug.Log("Network started");
        _networkManager = InstanceFinder.NetworkManager;
        _networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
        //InitializePlayers();
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        //create player for self
        //NetworkConnection networkConnection = _networkManager.ClientManager.Connection;
        //CreatePlayer(networkConnection);
        //Debug.Log("inside on client loaded start scene steam doesn't seem to call this one..");

        //Player tempPlayer = new Player
        //{
        //    clientID = networkConnection.ClientId,
        //    lives = GameManager.initialLivesCount,
        //    slayers = new Dictionary<string, int>(),
        //    victims = new Dictionary<string, int>(),
        //    bullets = defaultBulletCount,
        //    isReloading = false,
        //    networkCOnnection = networkConnection,
        //};
        //tempPlayer.steamName = networkConnection.ClientId.ToString();

        //if (SteamAPI.Init())
        //{
        //    int lobbyMemberCount = SteamMatchmaking.GetNumLobbyMembers(new CSteamID(BootstrapManager.CurrentLobbyID));
        //    CSteamID tempSteamID = (CSteamID)SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(BootstrapManager.CurrentLobbyID), lobbyMemberCount - 1);
        //    tempPlayer.steamName = SteamFriends.GetFriendPersonaName(tempSteamID);
        //}
        //else
        //{
        //    tempPlayer.steamName = networkConnection.ClientId.ToString();
        //}
        //players.Add(tempPlayer);
        //players.DirtyAll();
    }
    void InitializePlayers()
    {
        if (!IsServer) return;

        //_networkManager.ClientManager.Clients
        foreach(var client in _networkManager.ClientManager.Clients)
        {
            //client.Key
            NetworkConnection networkConnection = client.Value;

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
            tempPlayer.steamName = networkConnection.ClientId.ToString();

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
            players.DirtyAll();

        }


    }
    /// <summary>
    /// Gets the index of a player in the players list based on the provided client ID.
    /// </summary>
    /// <param name="id">The client ID to match.</param>
    /// <returns>
    /// The index of the player with the specified client ID if found; otherwise, returns -1.
    /// </returns>
    public int GetPlayerMatchingIDIndex(int id)
    {
        int matchingID = -1;
        for (int i = 0; i< players.Count; i++)
        {
            Player player = players[i];
            if(player.clientID == id)
            {
                return i;
            }
        }
        return matchingID;
    }
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
    /// <summary>
    /// Server-side method to reset player attributes at the start of a match.
    /// </summary>
    private void OnEnable()
    {
        GameManager.OnStartMatch += ResetPlayers;
    }
    private void OnDisable()
    {
        GameManager.OnStartMatch-= ResetPlayers;
    }
    /// <summary>
    /// Server-side method to reset all players at the start of a match.
    /// </summary>
    [Server]
    public void ResetPlayers()
    {
        Debug.Log("reset players..");
        for(int i =0; i<players.Count; i++)
        {
            players[i] = GetResetPlayer(players[i].steamName, players[i].clientID);
        }
        players.DirtyAll();
    }
    Player GetResetPlayer(string _name,int _clientID)
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
    /// <summary>
    /// Server-side method to update kill records, decrement victim lives, and update slayer and victim dictionaries. Triggers match end if only one live player remains.
    /// </summary>
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
    /// <summary>
    /// Updates the count of a key in a dictionary, creating the key if it doesn't exist.
    /// </summary>
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
    /// <summary>
    /// Returns a random spawn location for a player.
    /// </summary>
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
    /// <summary>
    /// Creates and spawns a player for the given network connection.
    /// </summary>
    /// <param name="networkConnection">The network connection for the player.</param>
    void CreatePlayer(NetworkConnection networkConnection)
    {
        NetworkObject networkOb = _networkManager.GetPooledInstantiated(playerPrefab, playerPrefab.transform.position, playerPrefab.transform.rotation, true);
        _networkManager.ServerManager.Spawn(networkOb, networkConnection);
        networkOb.gameObject.name = networkOb.gameObject.name + networkConnection;
        //choose random presetspawnlocation for gameobject
        _networkManager.SceneManager.AddOwnerToDefaultScene(networkOb);
        networkOb.gameObject.transform.position = GetRandomSpawnLocation().transform.position;

        networkOb.GetComponent<Health>().ownerID = networkConnection.ClientId;
        Debug.Log("create player");
    }
    private void SceneManager_OnClientLoadedStartScenes(NetworkConnection networkConnection, bool asServer)
    {
        if (!asServer)
            return;
        //if(!SteamAPI.Init())
        CreatePlayer(networkConnection);
        Debug.Log("inside on client loaded start scene steam doesn't seem to call this one..");

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
        tempPlayer.steamName = networkConnection.ClientId.ToString();

        //if (SteamAPI.Init())
        //{
        //    int lobbyMemberCount = SteamMatchmaking.GetNumLobbyMembers(new CSteamID(BootstrapManager.CurrentLobbyID));
        //    CSteamID tempSteamID = (CSteamID)SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(BootstrapManager.CurrentLobbyID), lobbyMemberCount - 1);
        //    tempPlayer.steamName = SteamFriends.GetFriendPersonaName(tempSteamID);
        //}
        //else
        //{
        //tempPlayer.steamName = networkConnection.ClientId.ToString();
        //}
        players.Add(tempPlayer);
        players.DirtyAll();
    }
}
