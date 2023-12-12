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
    string msg;
    //method shall be used to fill the lobby
    public override void OnStartClient()
    {
        base.OnStartClient();
        int lobbyCount = SteamMatchmaking.GetNumLobbyMembers(new CSteamID(BootstrapManager.CurrentLobbyID));
        CSteamID tempSteamID = SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(BootstrapManager.CurrentLobbyID), 
            lobbyCount);
        UpdateLobbyList(SteamFriends.GetFriendPersonaName(tempSteamID));
    }
    [ServerRpc(RequireOwnership = false)]
    void UpdateLobbyList(string thing)
    {
        UpdateLobbyListObserver(thing);
    }
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), msg);
    }
    [ObserversRpc]
    void UpdateLobbyListObserver(string thing)
    {
        msg = "called observer";
        Debug.Log("call  on all observers..");
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