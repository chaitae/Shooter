using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerEntryController
{
    Label playerNameL;
    public void SetVisualElement(VisualElement visualElement)
    {
        playerNameL = visualElement.Q<Label>("playerName");
    }
    public void SetNameLabel(string str)
    {
        playerNameL.text = str;
    }
}
