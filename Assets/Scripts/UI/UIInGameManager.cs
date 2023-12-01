using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UIInGameManager : MonoBehaviour
{
    [SerializeField] UIDocument leaderBoard;
    [SerializeField] VisualTreeAsset playerItemTemplate; 
    ListView leftList,rightList;
    public GameObject crossHair;
    public static UIInGameManager instance;
    private void Awake()
    {
        if(instance == null)
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
        PlayerManager.OnLeaderBoardDataChanged += UpdateLeaderBoard;

    }
    private void OnDisable()
    {
        PlayerManager.OnLeaderBoardDataChanged -= UpdateLeaderBoard;

    }
    public void UpdateLeaderBoard()
    {
        var lPlayers = PlayerManager.instance.players
            .Where((item,index) => index%2 != 0).ToList();

        var rPlayers = PlayerManager.instance.players
            .Where((item, index) => index % 2 == 0).ToList();

        //TODO: lPlayers doesn't seem to get victims?
        leftList.bindItem = (item, index) =>
        {
            int victCount = (lPlayers[index].victims == null) ? 0:lPlayers[index].victims.Count;
            int slayersCount = (lPlayers[index].slayers == null) ? 0 : lPlayers[index].slayers.Count;
            string sName = (lPlayers[index].steamName == null) ? lPlayers[index].clientID+"" : lPlayers[index].steamName;
            Debug.Log(lPlayers[index].clientID + "clientID");
            (item.userData as PlayerScoreEntryController).SetPlayerStats(lPlayers[index].steamName, slayersCount, victCount);
        };
        rightList.bindItem = (item, index) =>
        {
            int victCount = (rPlayers[index].victims == null) ? 0 : rPlayers[index].victims.Count;
            int slayersCount = (rPlayers[index].slayers == null) ? 0 : rPlayers[index].slayers.Count;
            string sName = (lPlayers[index].steamName == null) ? rPlayers[index].clientID + "" : rPlayers[index].steamName;
            (item.userData as PlayerScoreEntryController).SetPlayerStats(rPlayers[index].steamName,slayersCount, victCount);

        };
        leftList.itemsSource = lPlayers;
        rightList.itemsSource = rPlayers;
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
        if(Input.GetKey(KeyCode.Tab))
        {
            ShowLeaderBoard();
        }
        else
        {
            HideLeaderBoard();
        }
    }
}
