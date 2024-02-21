using System;
using System.Linq;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[XmlRoot("versionData")]
public class VersionData
{
    public string gameVersion;
    public string buildVersion;
    public DateTime releaseDate;
    public string releaseNote;
    public string News => releaseNote.TrimEmpty(false);
    
    public int keywordCount, traitCount;
    public int cardCount;
    public string cardPack;
    [XmlIgnore] public List<int> NewPackIds => cardPack.ToIntList('/');

    [XmlArray("topicDeck"), XmlArrayItem(typeof(Deck), ElementName = "deck")]
    public List<Deck> topicDecks = new List<Deck>();

    public bool IsEmpty() {
        return string.IsNullOrEmpty(gameVersion);
    }

    public static bool IsNullOrEmpty(VersionData versionData) {
        return (versionData == null) || (versionData.IsEmpty());
    }

    public static int CompareTo(string lhsVersion, string rhsVersion) {
        decimal lhs = 0, rhs = 0;
        if (!decimal.TryParse(lhsVersion, out lhs) || !decimal.TryParse(rhsVersion, out rhs))
            return 0;

        return lhs.CompareTo(rhs);
    }
}
