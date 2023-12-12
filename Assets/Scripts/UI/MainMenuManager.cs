using System;
using System.Collections.Generic;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager instance;

    [SerializeField] private GameObject menuScreen, lobbyScreen;
    [SerializeField] private TMP_InputField lobbyInput;

    [SerializeField] private TextMeshProUGUI lobbyTitle, lobbyIDText;
    [SerializeField] private Button startGameButton;
    [SerializeField] private GameObject playerSlot;
    [SerializeField] private GameObject playerSlotParent;
    string msg = "";

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        OpenMainMenu();
    }
    public void CopyLobbyID()
    {
        GUIUtility.systemCopyBuffer = BootstrapManager.CurrentLobbyID.ToString();

    }
    public void CreateLobby()
    {
        BootstrapManager.CreateLobby();
    }

    public void OpenMainMenu()
    {
        CloseAllScreens();
        menuScreen.SetActive(true);
    }

    public void OpenLobby()
    {
        CloseAllScreens();
        lobbyScreen.SetActive(true);
    }

    public static void LobbyEntered(string lobbyName, bool isHost)
    {
        instance.lobbyTitle.text = lobbyName;
        instance.startGameButton.gameObject.SetActive(isHost);
        instance.lobbyIDText.text = BootstrapManager.CurrentLobbyID.ToString();
        instance.OpenLobby();
    }

    void CloseAllScreens()
    {
        menuScreen.SetActive(false);
        lobbyScreen.SetActive(false);
    }

    public void JoinLobby()
    {
        CSteamID steamID = new CSteamID(Convert.ToUInt64(lobbyInput.text));
        BootstrapManager.JoinByID(steamID);
    }

    public void LeaveLobby()
    {
        BootstrapManager.LeaveLobby();
        OpenMainMenu();
    }

    public void StartGame()
    {
        string[] scenesToClose = new string[] { "MenuSceneSteam" };
        BootstrapNetworkManager.ChangeNetworkScene("SteamGameScene", scenesToClose);
    }

    public void UpdateLobbyList(string playerName)
    {
        msg = playerName;
        GameObject go = Instantiate(playerSlot, playerSlotParent.transform);
        TextMeshPro tmp;
        go.TryGetComponent<TextMeshPro>(out tmp);
        if(tmp!= null)
        {
            tmp.text = playerName;
            Debug.Log(playerName + "setting player name");

        }
    }
}

