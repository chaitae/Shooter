using System.Linq;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using Steamworks;
using UnityEngine;

public class BootstrapNetworkManager : NetworkBehaviour
{
    private static BootstrapNetworkManager instance;
    private void Awake() => instance = this;
    //method shall be used to fill the lobby
    public override void OnStartClient()
    {
        base.OnStartClient();
        CSteamID tempSteamID = SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(BootstrapManager.CurrentLobbyID), 
            0);
        UpdateLobbyList(SteamFriends.GetFriendPersonaName(tempSteamID));
    }
    [ServerRpc(RequireOwnership = false)]
    void UpdateLobbyList(string thing)
    {
        UpdateLobbyListObserver(thing);
    }
    [ObserversRpc]
    void UpdateLobbyListObserver(string thing)
    {
        MainMenuManager.instance.UpdateLobbyList(thing);
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