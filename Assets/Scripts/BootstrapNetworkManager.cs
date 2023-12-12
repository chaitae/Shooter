using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using Steamworks;
using UnityEngine;

public class BootstrapNetworkManager : NetworkBehaviour
{
    public static BootstrapNetworkManager instance;
    private void Awake() => instance = this;
    public List<string> lobbyNames = new List<string>();

    //method shall be used to fill the lobby
    public override void OnStartClient()
    {
        base.OnStartClient();
    }
    [ServerRpc(RequireOwnership = false)]
    public void UpdateLobbyList(string playerName)
    {
        DebugGUI.LogMessage("updatelobbylist");
        UpdateLobbyListObserver(playerName);
    }
    [ObserversRpc]
    void UpdateLobbyListObserver(string playerName)
    {
        DebugGUI.LogMessage("inside update lobbylist observer");
        lobbyNames.Add(playerName);
        lobbyNames.ForEach((lName) => DebugGUI.LogMessage(lName));
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