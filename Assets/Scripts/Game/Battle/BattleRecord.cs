using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class BattleRecord
{
    public bool isMaster;
    public BattleResultState resultState = BattleResultState.Draw;
    public BattleSettings settings;
    public Deck masterDeck, clientDeck;

    [XmlArray("actionList"), XmlArrayItem(typeof(IKeyValuePair<int[], bool>), ElementName = "action")] 
    public List<IKeyValuePair<int[], bool>> actionList = new List<IKeyValuePair<int[], bool>>();
    public DateTime date;

    public BattleRecord(){}

    public void AddAction(int[] action, bool isMe) {
        actionList.Add(new IKeyValuePair<int[], bool>(action, isMe));
    }
}
