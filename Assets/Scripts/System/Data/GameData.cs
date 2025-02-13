using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[XmlRoot("gameData")]
public class GameData
{
    public string nickname;
    public float BGMVolume, SEVolume;
    public bool turnEndHint;

    [XmlArray("deckList"), XmlArrayItem(typeof(Deck), ElementName = "deck")]
    public List<Deck> decks;

    [XmlArray("battleRecordList"), XmlArrayItem(typeof(BattleRecord), ElementName = "battleRecord")]
    public List<BattleRecord> battleRecords = new List<BattleRecord>();

    public GameData() {
        
    }

    public static GameData GetDefaultData() {
        GameData data = new GameData();
        data.InitGameData();
        return data;
    }

    public void InitGameData() {
        nickname = string.Empty;
        decks = new List<Deck>();
        battleRecords = new List<BattleRecord>();
        BGMVolume = SEVolume = 10f;
        turnEndHint = false;
    }

    public GameData Verify() {
        for (int i = 0; i < decks.Count; i++) {
            if ((decks[i].battles == null) || (decks[i].battles.Count != 9))
                decks[i].battles = Enumerable.Repeat(0, 9).ToList();

            if ((decks[i].wins == null) || (decks[i].wins.Count != 9))
                decks[i].wins = Enumerable.Repeat(0, 9).ToList();
        }
        return this;
    }

    public bool IsEmpty() {
        return string.IsNullOrEmpty(nickname);
    }

    public void SetNickname(string name) {
        nickname = name;
    }
}
