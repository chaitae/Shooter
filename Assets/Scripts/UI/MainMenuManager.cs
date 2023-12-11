using System;
using System.Collections.Generic;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
//using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private static MainMenuManager instance;

    [SerializeField] private TMP_InputField lobbyInput;
    [SerializeField] private TextMeshProUGUI lobbyTitle, lobbyIDText;
    [SerializeField] UIDocument mainScreen;
    [SerializeField] UIDocument lobbyScreen;
    [SerializeField] UIDocument enterLobbyID;
    [SerializeField] VisualElement visualElement;
    [SerializeField] VisualTreeAsset playerItemTemplate;
    ListView playerList;
    private void Awake() => instance = this;

    private void Start()
    {
        visualElement = mainScreen.rootVisualElement;
        lobbyScreen.rootVisualElement.Q<Button>("CopyID").clicked += CopyID;
        visualElement.Q<Button>("HostButton").clicked += CreateLobby;
        visualElement.Q<Button>("JoinButton").clicked += ShowJoinScreen;
        VisualElement visualElement2 = enterLobbyID.rootVisualElement;
        visualElement2.Q<Button>("enterIDButton").clicked += JoinLobby;
        lobbyScreen.rootVisualElement.Q<Button>("StartGame").clicked += StartGame;
        playerList = lobbyScreen.rootVisualElement.Q<ListView>("playerList");
        playerList.makeItem = () =>
        {
            var newPlayerEntry = playerItemTemplate.Instantiate();
            var newPlayerEntryLogic = new PlayerEntryController();
            newPlayerEntry.userData = newPlayerEntryLogic;
            newPlayerEntryLogic.SetVisualElement(newPlayerEntry);
            return newPlayerEntry;
        };
        OpenMainMenu();

    }

    private void CopyID()
    {
        GUIUtility.systemCopyBuffer = BootstrapManager.CurrentLobbyID.ToString();
    }

    void ShowJoinScreen()
    {
        Debug.Log("inside join screen");
        CloseAllScreens();
        enterLobbyID.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    public void CreateLobby()
    {
        BootstrapManager.CreateLobby();
    }

    public void OpenMainMenu()
    {
        CloseAllScreens();
        mainScreen.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    public void OpenLobby()
    {
        CloseAllScreens();
        lobbyScreen.rootVisualElement.style.display = DisplayStyle.Flex;

        instance.visualElement = instance.lobbyScreen.rootVisualElement;
        instance.visualElement.Q<Label>("lobbyID").text = "ID: " + BootstrapManager.CurrentLobbyID.ToString();
    }

    public static void LobbyEntered(string lobbyName, bool isHost,List<string> lobbyMembers)
    {
        //instance.startGameButton.gameObject.SetActive(isHost);
        Debug.Log("created lobby:" + BootstrapManager.CurrentLobbyID.ToString());
        Debug.Log(lobbyName+ " " + isHost);
        instance.OpenLobby();
        instance.FillLobby(lobbyMembers);
        instance.playerList.itemsSource = lobbyMembers;
        if (!isHost)
            instance.lobbyScreen.rootVisualElement.Q<Button>("StartGame").style.display = DisplayStyle.None;
    }
    void FillLobby(List<string> lobbyMembers)
    {
        playerList.bindItem = (item, index) =>
        {
            (item.userData as PlayerEntryController).SetNameLabel(lobbyMembers[index]);
        };
    }
    void CloseAllScreens()
    {

        enterLobbyID.rootVisualElement.style.display = DisplayStyle.None;
        mainScreen.rootVisualElement.style.display = DisplayStyle.None;
        lobbyScreen.rootVisualElement.style.display = DisplayStyle.None;
    }
    public void JoinLobby()
    {
        string textInput = enterLobbyID.rootVisualElement.Q<TextField>("inputText").text;
        CSteamID steamID = new CSteamID(Convert.ToUInt64(textInput));
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
        BootstrapNetworkManager.ChangeNetworkScene("Test", scenesToClose);
    }
}
