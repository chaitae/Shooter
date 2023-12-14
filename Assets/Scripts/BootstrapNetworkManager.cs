using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

public class BootstrapNetworkManager : NetworkBehaviour
{
    private static BootstrapNetworkManager instance;
    [SerializeField] GameObject gameManagerPrefab, UIMenuPrefab, playerManagerPrefab;

    private void Awake() => instance = this;

    public override void OnStartClient()
    {
        base.OnStartClient();
        base.OnStartServer();
        UpdateLobbyList();

    }
    [ServerRpc(RequireOwnership = false)]
    public void UpdateLobbyList()
    {
        UpdateLobbyListObserver();
    }
    [ObserversRpc(ExcludeOwner = true)]
    public void UpdateLobbyListObserver()
    {
        Debug.Log("observer");
        DebugGUI.LogMessage("hello register message");
        BootstrapManager.instance.UpdateLobbyList();

        //tell menu manager update
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
