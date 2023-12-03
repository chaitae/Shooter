using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UIInGameManager : MonoBehaviour
{
    [SerializeField] UIDocument leaderBoard;
    [SerializeField] VisualTreeAsset playerItemTemplate;
    ListView leftList, rightList;
    public GameObject crossHair;
    public static UIInGameManager instance;
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

        leftList.makeItem = () =>
        {
            var newPlayerScoreEntry = playerItemTemplate.Instantiate();
            var newPlayerScoreEntryLogic = new PlayerScoreEntryController();
            newPlayerScoreEntry.userData = newPlayerScoreEntryLogic;
            newPlayerScoreEntryLogic.SetVisualElement(newPlayerScoreEntry);
            return newPlayerScoreEntry;
        };
        rightList.makeItem = () =>
        {
            var newPlayerScoreEntry = playerItemTemplate.Instantiate();
            var newPlayerScoreEntryLogic = new PlayerScoreEntryController();
            newPlayerScoreEntry.userData = newPlayerScoreEntryLogic;
            newPlayerScoreEntryLogic.SetVisualElement(newPlayerScoreEntry);
            return newPlayerScoreEntry;
        };
        // Subscribe to events for leaderboard data changes and player list changes.
        PlayerManager.OnLeaderBoardDataChanged += UpdateLeaderBoard;
        PlayerManager.instance.players.OnChange += PlayersOnChange;

    }

    private void PlayersOnChange(SyncListOperation op, int index, Player oldItem, Player newItem, bool asServer)
    {
        UpdateLeaderBoard();
    }

    private void OnDisable()
    {
        PlayerManager.OnLeaderBoardDataChanged -= UpdateLeaderBoard;

    }
    public void UpdateLeaderBoard()
    {
        // Split players into dif teams
        var lPlayers = PlayerManager.instance.players
        .Where((item, index) => (index % 2 == 0));
        var rPlayers = PlayerManager.instance.players
        .Where((item, index) => (index % 2 != 0));
        leftList.bindItem = (item, index) =>
        {
            int deathCount =lPlayers
            .SelectMany(player => player.slayers.Values)
            .Sum();

            int killCount = lPlayers
            .SelectMany(player => player.victims.Values)
            .Sum();
            string steamName = (lPlayers.ToList()[index].steamName == null) ? "" : lPlayers.ToList()[index].steamName;
            (item.userData as PlayerScoreEntryController).SetPlayerStats(steamName, deathCount, killCount);
        };
        rightList.bindItem = (item, index) =>
        {
            int deathCount = PlayerManager.instance.players
            .SelectMany(player => player.slayers.Values)
            .Sum();

            int killCount = PlayerManager.instance.players
            .SelectMany(player => player.victims.Values)
            .Sum();
            string steamName = (lPlayers.ToList()[index].steamName == null) ? "" : lPlayers.ToList()[index].steamName;
            (item.userData as PlayerScoreEntryController).SetPlayerStats(steamName, deathCount, killCount);

        };
        leftList.itemsSource = lPlayers.ToList();
        rightList.itemsSource = rPlayers.ToList();
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
