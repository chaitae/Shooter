using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerScoreEntryController
{
    Label playerName;
    Label kda,kills;

    public void SetVisualElement(VisualElement visualElement)
    {
        playerName = visualElement.Q<Label>("PlayerText");
        kda = visualElement.Q<Label>("KDA");
        //kills = visualElement.Q<Label>("NumKills");
    }
    public void SetPlayerStats(string _playerName,int _deaths,int _kills)
    {
        playerName.text = _playerName;
        kda.text = _kills+ "/"+_deaths;
        //kills.text = _kills + "";
    }
}
