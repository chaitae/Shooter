using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerScoreEntryController
{
    Label playerName;
    Label deaths,kills;

    public void SetVisualElement(VisualElement visualElement)
    {
        playerName = visualElement.Q<Label>("PlayerText");
        deaths = visualElement.Q<Label>("NumDeaths");
        kills = visualElement.Q<Label>("NumKills");
    }
    public void SetPlayerStats(string _playerName,int _deaths,int _kills)
    {
        playerName.text = _playerName;
        deaths.text = _kills+ "/"+_deaths;
        //kills.text = _kills + "";
    }
}
