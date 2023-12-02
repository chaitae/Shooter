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
    public int maxRounds = 12; // Maximum number of rounds in a match
    public int currentRound = 1; // Current round number
    public Text roundTimerText; // Reference to UI text for round timer

    private bool isRoundActive = false; // Flag to track if the round is active
    private float roundTimer; // Timer for the round
    [SyncVar]
    public string msg;
    public GameObject playerPrefab;
    NetworkManager _networkManager;

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

    void Update()
    {
        if (isRoundActive)
        {
            UpdateRoundTimer();
        }
        
    }

    void StartRound()
    {
        isRoundActive = true;
        roundTimer = roundTime;
        //UpdateRoundUI();
        // Other initialization for the round (e.g., spawning players)
    }
    [ServerRpc(RequireOwnership = false)]
    void RPCEndRound()
    {
        isRoundActive = false;
        msg = "endround";
        // Logic for ending the round, declaring winners, etc.
        currentRound++;
        if (currentRound <= maxRounds)
        {
            StartRound(); // Start the next round
        }
        else
        {
            // Logic for ending the match, showing game over screen, etc.
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateRoundTimer()
    {
        roundTimer -= Time.deltaTime;
        if (roundTimer <= 0f)
        {
            roundTimer = 0f;
            RPCEndRound();
        }
        //UpdateRoundUI();
    }

    void UpdateRoundUI()
    {
        int minutes = Mathf.FloorToInt(roundTimer / 60f);
        int seconds = Mathf.FloorToInt(roundTimer % 60f);
        string timerString = string.Format("{0:0}:{1:00}", minutes, seconds);
        roundTimerText.text = timerString;
    }
}
