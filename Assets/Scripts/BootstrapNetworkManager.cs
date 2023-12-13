using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Steamworks;
using UnityEngine;

public class BootstrapNetworkManager : NetworkBehaviour
{
    public static BootstrapNetworkManager instance;
    private void Awake() => instance = this;
    [SyncObject]
    public readonly SyncList<string> lobbyNames = new SyncList<string>();

    //method shall be used to fill the lobby
    public override void OnStartClient()
    {
        base.OnStartClient();
        DebugGUI.LogMessage("Client started! BootStrapManager");

        // Get the number of lobby members
        int countMembers = SteamMatchmaking.GetNumLobbyMembers(new CSteamID(BootstrapManager.CurrentLobbyID));
        DebugGUI.LogMessage($"Lobby member count: {countMembers}");

        // Add the latest Steam friend to the lobbyNames list
        string latestMember = SteamFriends.GetFriendPersonaName(SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(BootstrapManager.CurrentLobbyID), countMembers - 1));
        lobbyNames.Add(latestMember);
        lobbyNames.Dirty(lobbyNames.Count - 1);

        // Update the lobby list
        UpdateLobbyList();

        // Log the client count
        DebugGUI.LogMessage($"{base.NetworkManager.ClientManager.Clients.Count} client(s) connected");
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateLobbyList()
    {
        DebugGUI.LogMessage("Inside Update LobbyList");
        UpdateLobbyListObserver();
    }

    [ObserversRpc]
    void UpdateLobbyListObserver()
    {
        DebugGUI.LogMessage("Inside UpdateLobbyListObserver");
        DebugGUI.LogMessage("List of people:");

        foreach (var member in lobbyNames)
        {
            DebugGUI.LogMessage(member.ToString());
        }
        // Alternatively, you can use string.Join to concatenate the list elements
        // DebugGUI.LogMessage("List of people: " + string.Join(", ", lobbyNames));
    }
    public static void ChangeNetworkScene(string sceneName, string[] scenesToClose)
    {
        instance.CloseScenes(scenesToClose);

        SceneLoadData sld = new SceneLoadData(sceneName);
        NetworkConnection[] conns = instance.ServerManager.Clients.Values.ToArray();
        instance.SceneManager.LoadConnectionScenes(conns, sld);
    }

    [ServerRpc(RequireOwnership = false)]
    void CloseScenes(string[] scenesToClose)
    {
        CloseScenesObserver(scenesToClose);
    }

    [ObserversRpc]
    void CloseScenesObserver(string[] scenesToClose)
    {
        foreach (var sceneName in scenesToClose)
        {
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
        }
    }
}