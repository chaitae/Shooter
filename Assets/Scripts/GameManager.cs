using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System;
using System.Linq;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    public static int initialLivesCount = 3;
    public GameObject playerPrefab;
    NetworkManager _networkManager;
    public static Action OnEndMatch,OnStartMatch;
    private bool isRoundActive = false; 
    public string winner;

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
        SetWinner();
        OnEndMatch?.Invoke();
        //winner is the last person alive
    }
    [ObserversRpc]
    public void SetWinner()
    {
        var lPlayers = PlayerManager.instance.players
        .Where((item, index) => (index % 2 == 0 && item.lives > 0));
        var rPlayers = PlayerManager.instance.players
        .Where((item, index) => (index % 2 != 0 && item.lives > 0));
        if(lPlayers.Count() > 0)
        {
            winner = "BlueTeam Wins!";
        }
        else
        {
            winner = "RedTeam Wins!";
        }
    }

}
