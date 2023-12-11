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
    ListView leftList, rightList,leftList2,rightList2;
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
        leaderBoard.rootVisualElement.style.display = DisplayStyle.None;
        leftList = leaderBoard.rootVisualElement.Q<ListView>("LeftPlayerList");
        rightList = leaderBoard.rootVisualElement.Q<ListView>("RightPlayerList");
        leftList2 = endMatchScreen.rootVisualElement.Q<ListView>("LeftPlayerList");
        rightList2 = endMatchScreen.rootVisualElement.Q<ListView>("RightPlayerList");
        winHeader = endMatchScreen.rootVisualElement.Q<Label>("WinHeader");

        leftList.makeItem = MakeScoreItem;
        rightList.makeItem = MakeScoreItem;
        leftList2.makeItem = MakeScoreItem;
        rightList2.makeItem = MakeScoreItem;

        InitializeEndMatchScreenButtons();
        HideEndMatchScreen();
        // Subscribe to events for leaderboard data changes and player list changes.
        PlayerManager.OnLeaderBoardDataChanged += UpdateLeaderBoard;
        PlayerManager.instance.players.OnChange += PlayersOnChange;
        GameManager.OnEndMatch += ShowEndMatchScreen;
        GameManager.OnStartMatch += ResetMenu;
    }


    private void ResetMenu()
    {
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
    }
    TemplateContainer MakeScoreItem()
    {
        var newPlayerScoreEntry = playerItemTemplate.Instantiate();
        var newPlayerScoreEntryLogic = new PlayerScoreEntryController();
        newPlayerScoreEntry.userData = newPlayerScoreEntryLogic;
        newPlayerScoreEntryLogic.SetVisualElement(newPlayerScoreEntry);
        return newPlayerScoreEntry;
    }

    private void PlayersOnChange(SyncListOperation op, int index, Player oldItem, Player newItem, bool asServer)
    {
        if(asServer)
        {
            return;
        }

        if (base.ClientManager.Connection.ClientId == newItem.clientID)
        {
            UpdateLocalBulletsandHealth(base.ClientManager.Connection);
            //UpdateLocalHealthGUI(base.ClientManager.Connection, newItem.lives);
        }
        UpdateLeaderBoard();
    }
    [ServerRpc(RequireOwnership = false)]
    void UpdateLocalBulletsandHealth(NetworkConnection networkCOnnection)
    {
        UpdateLocalBulletGUI(networkCOnnection);
        UpdateLocalHealthGUI(networkCOnnection);


    }
    //figure out how to run the below in sersver
    [TargetRpc]
    private void UpdateLocalBulletGUI(NetworkConnection networkCOnnection)
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
    private void UpdateLocalHealthGUI(NetworkConnection conn)
    {
        int hiddenCount = PlayerManager.defaultLivesCount - PlayerManager.instance.players[PlayerManager.instance.GetPlayerMatchingIDIndex(conn.ClientId)].lives;
        for(int i = 0; i<hiddenCount; i++)
        {
            lifeIcons[i].gameObject.SetActive(false);
        }
        //This might be something you only want the owner to be aware of.
    }
    private void OnDisable()
    {
        PlayerManager.OnLeaderBoardDataChanged -= UpdateLeaderBoard;
        GameManager.OnEndMatch -= ShowEndMatchScreen;
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
    public void UpdateLeaderBoard()
    {
        // Split players into dif teams
        var lPlayers = PlayerManager.instance.players
        .Where((item, index) => (index % 2 == 0));
        var rPlayers = PlayerManager.instance.players
        .Where((item, index) => (index % 2 != 0));
        BindPlayerStats(leftList.bindItem,lPlayers);
        leftList.bindItem = BindPlayerStats(leftList.bindItem, lPlayers);
        rightList.bindItem = BindPlayerStats(rightList.bindItem, rPlayers);
        leftList2.bindItem = BindPlayerStats(leftList.bindItem, lPlayers);
        rightList2.bindItem = BindPlayerStats(rightList.bindItem, rPlayers);
        leftList.itemsSource = lPlayers.ToList();
        rightList.itemsSource = rPlayers.ToList();
        leftList2.itemsSource= rPlayers.ToList();
        rightList2.itemsSource = rPlayers.ToList();
    }
    public void ShowLeaderBoard()
    {
        leaderBoard.rootVisualElement.style.display = DisplayStyle.Flex;
        crossHair.gameObject.SetActive(false);
    }
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
        winHeader.text = GameManager.instance.winner;
        endMatchScreen.rootVisualElement.style.display = DisplayStyle.Flex;
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
