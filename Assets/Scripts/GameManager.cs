using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using System.Collections.Generic;
using System;
using System.Linq;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    public int roundTime = 180; // Round time in seconds
    //public int maxRounds = 12; // Maximum number of rounds in a match
    //public int currentRound = 1; // Current round number
    public Text roundTimerText; // Reference to UI text for round timer

    private bool isRoundActive = false; // Flag to track if the round is active
    private float roundTimer; // Timer for the round
    [SyncVar]
    public string msg;
    public static int initialLivesCount = 3;
    public GameObject playerPrefab;
    NetworkManager _networkManager;
    public static Action OnEndMatch,OnStartMatch;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width/3, Screen.height/3, 100, 20), msg);
    }
    //TODO: Set up game state enums and actions for player to subscribe to
    //TODO: restart game new match
    public override void OnStartServer()
    {

        if (base.IsHost && base.IsServer)
            enabled = false;
        base.OnStartServer();
        PlayerManager.OnAddPlayer += InstantiatePlayers;
        //run when using steam lobby
        _networkManager = InstanceFinder.NetworkManager;
        for (int i = 0; i < PlayerManager.instance.players.Count; i++)
        {
            int tempID = PlayerManager.instance.players[i].clientID;
            NetworkConnection networkConnection = _networkManager.ClientManager.Clients[tempID];
            NetworkObject networkOb = _networkManager.GetPooledInstantiated(playerPrefab, playerPrefab.transform.position, playerPrefab.transform.rotation, true);
            _networkManager.ServerManager.Spawn(networkOb, networkConnection);

        }
        StartRound();
    }

    private void InstantiatePlayers(NetworkConnection networkConnection)
    {
        NetworkObject networkOb = _networkManager.GetPooledInstantiated(playerPrefab, playerPrefab.transform.position, playerPrefab.transform.rotation, true);
        _networkManager.ServerManager.Spawn(networkOb, networkConnection);
        _networkManager.SceneManager.AddOwnerToDefaultScene(networkOb);
    }



    void StartRound()
    {
        OnStartMatch?.Invoke();
        isRoundActive = true;
    }
    [ServerRpc(RequireOwnership = false)]
    public void RPCEndMatch()
    {
        isRoundActive = false;
        msg = "endround";
        OnEndMatch?.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateRoundTimer()
    {
        roundTimer -= Time.deltaTime;
        if (roundTimer <= 0f)
        {
            roundTimer = 0f;
        }
    }

    void UpdateRoundUI()
    {
        int minutes = Mathf.FloorToInt(roundTimer / 60f);
        int seconds = Mathf.FloorToInt(roundTimer % 60f);
        string timerString = string.Format("{0:0}:{1:00}", minutes, seconds);
        roundTimerText.text = timerString;
    }
}
