using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Deck
{   
    public static List<string> defaultDeckCreateNames => new List<string>(){ "創建牌組", "牌組代碼" };
    public static List<Deck> DefaultCreateDecks => defaultDeckCreateNames.Select(x => new Deck(){ name = x }).ToList();
    public static string Encode(Deck deck) {
        if (deck == null)
            return null;

        var code = Encrypter.IntToBase64String(deck.zone) + Encrypter.IntToBase64String(deck.format) + Encrypter.IntToBase64String(deck.craft);
        for (int i = 1; i <= 3; i++) {
            int copy = i;
            var cardIds = deck.CardIdDistribution.Where(x => x.Value == copy).ToList();
            code += Encrypter.IntToBase64String(cardIds.Count);
            code += cardIds.Select(x => Encrypter.IntToBase64String(x.Key)).ConcatToString(string.Empty);
        }
        return code;
    }

    public static Deck Decode(string deckCode) {
        if (string.IsNullOrEmpty(deckCode) || (deckCode.Length % 5 != 1))
            return null;

        var zone = (CardZone)Encrypter.Base64StringToInt(deckCode.Substring(0, 1));
        var format = (GameFormat)Encrypter.Base64StringToInt(deckCode.Substring(1, 1));
        var craft = (CardCraft)Encrypter.Base64StringToInt(deckCode.Substring(2, 1));
        var deck = new Deck(zone, format, craft);
        var cursor = 3;

        for (int i = 1; i <= 3; i++) {
            int copy = i;
            int count = Encrypter.Base64StringToInt(deckCode.Substring(cursor, 1));

            cursor++;
            for (int j = 0; j < count; j++) {
                if ((deckCode.Length - cursor) < 5)
                    return null;

                var cardId = Encrypter.Base64StringToInt(deckCode.Substring(cursor, 5));
                var card = Card.Get(cardId);
                if ((card == null) || (!card.IsFormat(format)) || ((card.Zone != zone) && (card.Zone != CardZone.Cygames)))
                    return null;

                deck.cardIds.AddRange(Enumerable.Repeat(cardId, copy));
                cursor += 5;
            }
        }
        deck.Sort();
        return deck;
    }

    public static Deck GetGemDeck(CardZone zone, CardCraft craft) {
        int gemId = 500001201;
        // int crystalId = 500003201;
        return new Deck(zone, GameFormat.GemOfFortune, craft) {
            name = craft.GetCraftName(),
            cardIds = Enumerable.Repeat(gemId, 30).ToList(),
        };
    }

    public string name;
    public int zone, format, craft;
    
    [XmlArray("cards"), XmlArrayItem(typeof(int), ElementName = "id")] 
    public List<int> cardIds;

    [XmlArray("battle"), XmlArrayItem(typeof(int), ElementName = "craft")] 
    public List<int> battles;

    [XmlArray("win"), XmlArrayItem(typeof(int), ElementName = "craft")] 
    public List<int> wins;

    [XmlIgnore] public DeckCreateType CreateType => (DeckCreateType)(IsEmpty() ? Deck.defaultDeckCreateNames.IndexOf(name) : -1); 
    [XmlIgnore] public string Code => Deck.Encode(this);
    [XmlIgnore] public int CardCount => cardIds.Count;
    [XmlIgnore] public int MaxCardCount => ((GameFormat)format).GetMaxCardCountInDeck();
    [XmlIgnore] public List<Card> Cards => cardIds.Select(Card.Get).ToList();
    [XmlIgnore] public List<Card> DistinctCards => cardIds.Distinct().Select(Card.Get).ToList();
    [XmlIgnore] public List<Card> DistinctNameCards => Cards.Select(x => x.NameId).Distinct().Select(Card.Get).ToList();
    [XmlIgnore] public Dictionary<int, int> CardIdDistribution => GetCardIdDistribution();
    [XmlIgnore] public Dictionary<int, int> CardNameIdDistribution => GetCardNameIdDistribution();
    [XmlIgnore] public List<int> CostDistribution => GetCostDistribution();
    [XmlIgnore] public int TotalBattles => battles.Sum();
    [XmlIgnore] public int TotalWins => wins.Sum();
    [XmlIgnore] public float WinRate => (TotalBattles == 0) ? 0f : (TotalWins * 1f / TotalBattles);

    public Deck() {}

    public Deck(CardZone zoneId, GameFormat formatId, CardCraft craftId) {
        zone = (int)zoneId;
        format = (int)formatId;
        craft = (int)craftId;
        cardIds = new List<int>();
        battles = Enumerable.Repeat(0, 9).ToList();
        wins = Enumerable.Repeat(0, 9).ToList();
    }

    public Deck(Deck rhs) {
        name = rhs.name;
        zone = rhs.zone;
        format = rhs.format;
        craft = rhs.craft;
        cardIds = new List<int>(rhs.cardIds);
        battles = new List<int>(rhs.battles);
        wins = new List<int>(rhs.wins);
    }
    public override string ToString()
    {
        return zone.ToString() + format.ToString() + craft.ToString() +
            cardIds.Select(x => x.ToString()).ConcatToString("/");
    }

    public bool IsUnnamed() {
        return string.IsNullOrEmpty(name);
    }

    public bool IsEmpty() {
        return ListHelper.IsNullOrEmpty(cardIds);
    }

    public bool IsDefault() {
        return IsUnnamed() || (CreateType != DeckCreateType.AlreadyCreated);
    }

    public bool IsBattleAvailable(CardZone battleZone, GameFormat gameFormat) {
        return (!IsUnnamed()) && (!IsEmpty()) && (CardCount == MaxCardCount)
            && (battleZone == (CardZone)zone) && (gameFormat == (GameFormat)format)
            && DistinctCards.All(x => (x != null) && (x.IsFormat(gameFormat)));
    }

    public Dictionary<int, int> GetCardIdDistribution() {
        Dictionary<int, int> idf = new Dictionary<int, int>();
        for (int i = 0; i < DistinctCards.Count; i++) {
            Card card = DistinctCards[i];
            idf.Add(card.id, cardIds.Count(id => card.id == id));
        }
        return idf;
    }

    public Dictionary<int, int> GetCardNameIdDistribution() {
        Dictionary<int, int> ndf = new Dictionary<int, int>();
        for (int i = 0; i < DistinctNameCards.Count; i++) {
            Card card = DistinctNameCards[i];
            ndf.Add(card.id, Cards.Count(x => card.id == x.NameId));
        }
        return ndf;
    }

    public List<int> GetCostDistribution() {
        var cdf = new List<int>() { Cards.Count(x => x.cost <= 1) };
        for (int i = 2; i <= 7; i++) {
            cdf.Add(Cards.Count(x => x.cost == i));
        }
        cdf.Add(Cards.Count(x => x.cost >= 8));
        return cdf;
    }

    public int GetTypeCount(CardType type) {
        return Cards.Count(x => x.Type == type);
    }

    public void Sort() {
        cardIds = Cards.OrderBy(CardDatabase.Sorter).Select(x => x.id).ToList();
    }

    public void AddCard(Card card) {
        cardIds.Add(card.id);
        Sort();
    }

    public void RemoveCard(Card card) {
        cardIds.Remove(card.id);
        Sort();
    }

}

public enum DeckCreateType {
    AlreadyCreated = -1,
    Normal = 0,
    Code = 1,
}
