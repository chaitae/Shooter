using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UIInGameManager : NetworkBehaviour
{
    [SerializeField] UIDocument leaderBoard,endMatchScreen;
    [SerializeField] VisualTreeAsset playerItemTemplate;
    ListView leftList, rightList,leftListEndMatch,rightListEndMatch;
    private Label winHeader;
    public GameObject crossHair;
    public static UIInGameManager instance;
    private Button playAgainButton;
    private Button quitButton;
    public GameObject[] lifeIcons,ammoIcons;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            GameObject.Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        // Hide the leaderboard at the start.
        //is this not being called for the built host?
        endMatchScreen.rootVisualElement.style.display = DisplayStyle.None;
        leaderBoard.rootVisualElement.style.display = DisplayStyle.None;
        leftList = leaderBoard.rootVisualElement.Q<ListView>("LeftPlayerList");
        rightList = leaderBoard.rootVisualElement.Q<ListView>("RightPlayerList");
        leftListEndMatch = endMatchScreen.rootVisualElement.Q<ListView>("LeftPlayerList");
        rightListEndMatch = endMatchScreen.rootVisualElement.Q<ListView>("RightPlayerList");
        winHeader = endMatchScreen.rootVisualElement.Q<Label>("WinHeader");

        //leftList.makeItem = MakeScoreItem;
        //rightList.makeItem = MakeScoreItem;
        //leftListEndMatch.makeItem = MakeScoreItem;
        //rightListEndMatch.makeItem = MakeScoreItem;

        InitializeEndMatchScreenButtons();
        HideEndMatchScreen();
        // Subscribe to events for leaderboard data changes and player list changes.
        PlayerManager.OnLeaderBoardDataChanged += UpdateLeaderBoardRPC;
        PlayerManager.instance.players.OnChange += PlayersOnChange;
        GameManager.OnEndMatch += ShowEndMatchScreen;
        GameManager.OnStartMatch += ResetMenuRPC;
    }
    private void OnDisable()
    {
        PlayerManager.OnLeaderBoardDataChanged -= UpdateLeaderBoardRPC;
        PlayerManager.instance.players.OnChange -= PlayersOnChange;
        GameManager.OnEndMatch -= ShowEndMatchScreen;
        GameManager.OnStartMatch -= ResetMenuRPC;
    }
    [ServerRpc(RequireOwnership =false)]
    private void ResetMenuRPC()
    {
        ResetMenuObserver();
    }
    /// <summary>
    /// Resets the menu by setting all life and ammo icons to active and hiding the end match screen.
    /// </summary>
    [ObserversRpc]
    private void ResetMenuObserver()
    {
        // need all observers
        for(int i =0; i<lifeIcons.Length; i++)
        {
            lifeIcons[i].SetActive(true);
        }
        for(int i =0; i<ammoIcons.Length;i++)
        {
            ammoIcons[i].SetActive(true);
        }
        HideEndMatchScreen();
    }
    /// <summary>
    /// Initializes the buttons on the end match screen, such as Play Again and Quit buttons.
    /// </summary>
    private void InitializeEndMatchScreenButtons()
    {
        if (endMatchScreen != null)
        {
            var root = endMatchScreen.rootVisualElement;

            playAgainButton = root.Q<Button>("PlayAgainButton");
            if (playAgainButton != null)
            {
                playAgainButton.clickable.clicked += OnPlayAgainClicked;
            }

            quitButton = root.Q<Button>("QuitButton");
            if (quitButton != null)
            {
                quitButton.clickable.clicked += OnQuitClicked;
            }
        }
    }

    private void OnPlayAgainClicked()
    {
        // Handle Play Again button click
        Debug.Log("Play Again Button Clicked");
        HideEndMatchScreen();
        GameManager.OnStartMatch?.Invoke();
    }

    private void OnQuitClicked()
    {
        // Handle Quit button click
        Debug.Log("Quit Button Clicked");
        Application.Quit();
        //TODO: maybe go back to lobby isntead and disconnect from lobby
    }
    TemplateContainer MakeScoreItem()
    {
        var newPlayerScoreEntry = playerItemTemplate.Instantiate();
        var newPlayerScoreEntryLogic = new PlayerScoreEntryController();
        newPlayerScoreEntry.userData = newPlayerScoreEntryLogic;
        newPlayerScoreEntryLogic.SetVisualElement(newPlayerScoreEntry);
        return newPlayerScoreEntry;
    }
    /// <summary>
    /// Callback method invoked when the players list changes. Updates local player's bullets and health if applicable, and refreshes the leaderboard.
    /// </summary>
    private void PlayersOnChange(SyncListOperation op, int index, Player oldItem, Player newItem, bool asServer)
    {
        if(asServer)
        {
            return;
        }

        if (base.ClientManager.Connection.ClientId == newItem.clientID)
        {
            UpdateLocalBulletsandHealthRPC(base.ClientManager.Connection);
        }
        UpdateLeaderBoardRPC();
    }
    [ServerRpc(RequireOwnership = false)]
    void UpdateLocalBulletsandHealthRPC(NetworkConnection networkCOnnection)
    {
        UpdateLocalBulletGUIRPC(networkCOnnection);
        UpdateLocalHealthGUIRPC(networkCOnnection);
    }
    //figure out how to run the below in sersver
    [TargetRpc]
    private void UpdateLocalBulletGUIRPC(NetworkConnection networkCOnnection)
    {
        for (int i = 0; i < ammoIcons.Length; i++)
        {
            if(i < PlayerManager.instance.players[PlayerManager.instance.GetPlayerMatchingIDIndex(networkCOnnection.ClientId)].bullets)
            {
                ammoIcons[i].SetActive(true);
            }
            else
            {
                ammoIcons[i].SetActive(false);
            }
        }
    }
    [TargetRpc]
    private void UpdateLocalHealthGUIRPC(NetworkConnection conn)
    {
        int hiddenCount = PlayerManager.defaultLivesCount - PlayerManager.instance.players[PlayerManager.instance.GetPlayerMatchingIDIndex(conn.ClientId)].lives;
        for(int i = 0; i<hiddenCount; i++)
        {
            lifeIcons[i].gameObject.SetActive(false);
        }
        //This might be something you only want the owner to be aware of.
    }
    Action<VisualElement, int> BindPlayerStats(Action<VisualElement,int> NewBindingAction, IEnumerable<Player> splitPlayers)
    {
        NewBindingAction = (item, index) =>
        {
            int deathCount = splitPlayers
            .SelectMany(player => player.slayers.Values)
            .Sum();
            int killCount = splitPlayers
            .SelectMany(player => player.victims.Values)
            .Sum();
            string steamName = (splitPlayers.ToList()[index].steamName == null) ? "" : splitPlayers.ToList()[index].steamName;
            (item.userData as PlayerScoreEntryController).SetPlayerStats(steamName, deathCount, killCount);
        };
        return NewBindingAction;
    }
    [ServerRpc(RequireOwnership = false)]
    public void UpdateLeaderBoardRPC()
    {
        //todo: you'll need to make an observer udpate rpc here
        // Split players into dif teams
        UpdateLeaderBoardObserver();
    }
    [ObserversRpc]
    public void UpdateLeaderBoardObserver()
    {
        var lPlayers = PlayerManager.instance.players
        .Where((item, index) => (index % 2 == 0));
        var rPlayers = PlayerManager.instance.players
        .Where((item, index) => (index % 2 != 0));

        DebugGUI.LogMessage(lPlayers.ElementAt(0).steamName + "  Left List" + lPlayers.ElementAt(0).lives + "/");
        if(rPlayers.Count() > 0)
        DebugGUI.LogMessage(rPlayers.ElementAt(0).steamName + " " + rPlayers.ElementAt(0).lives + "/");
        //BindPlayerStats(leftList.bindItem, lPlayers);
        //leftList.bindItem = BindPlayerStats(leftList.bindItem, lPlayers);
        //rightList.bindItem = BindPlayerStats(rightList.bindItem, rPlayers);

        //leftListEndMatch.bindItem = BindPlayerStats(leftList.bindItem, lPlayers);
        //rightListEndMatch.bindItem = BindPlayerStats(rightList.bindItem, rPlayers);

        //leftList.itemsSource = lPlayers.ToList();
        //rightList.itemsSource = rPlayers.ToList();

        //leftListEndMatch.itemsSource = rPlayers.ToList();
        //rightListEndMatch.itemsSource = rPlayers.ToList();
    }
    /// <summary>
    /// Displays the leaderboard by setting its visual style to flex and hiding the crosshair.
    /// </summary>
    public void ShowLeaderBoard()
    {
        //leaderBoard.rootVisualElement.style.display = DisplayStyle.Flex;
        crossHair.gameObject.SetActive(false);
    }
    /// <summary>
    /// Hides the leaderboard by setting its visual style to none and showing the crosshair.
    /// </summary>
    public void HideLeaderBoard()
    {
        leaderBoard.rootVisualElement.style.display = DisplayStyle.None;
        crossHair.gameObject.SetActive(true);

    }
    [ObserversRpc]
    public void ShowEndMatchScreen()
    {
        if (!base.IsHost)
        {
            playAgainButton.style.display = DisplayStyle.None;
        }
        //winHeader.text = GameManager.instance.winner;
        //endMatchScreen.rootVisualElement.style.display = DisplayStyle.Flex;
        crossHair.SetActive(false);
    }

    public void HideEndMatchScreen()
    {
        endMatchScreen.rootVisualElement.style.display = DisplayStyle.None;
        crossHair.SetActive(true);
    }
    private void Update()
    {
        if (Input.GetKey(KeyCode.Tab))
        {
            ShowLeaderBoard();
        }
        else
        {
            HideLeaderBoard();
        }
    }
}
