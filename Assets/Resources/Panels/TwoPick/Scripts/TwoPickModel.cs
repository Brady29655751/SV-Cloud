using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TwoPickModel : IMonoBehaviour
{

    public int Round => Player.currentDeck.CardCount / 2;
    public int MaxRound => Player.currentDeck.MaxCardCount / 2;

    public Card[] leftPair, rightPair;

    private List<Card> twoPickStorage = null;

    private CardFilter GetInitFilter() {
        return new CardFilter(Player.currentDeck.format) {
            zone = Player.currentDeck.zone,    
        };
    }

    public void InitDeck() {
        twoPickStorage = CardDatabase.CardMaster.Where(GetInitFilter().Filter).ToList();
    }

    public void GetNextPair() {
        if (Round >= MaxRound) {
            leftPair = rightPair = null;
            return;
        }
        var twoPick = GetTwoPickStorageByRound()?.Random(4, false);
        if (twoPick == null) {
            leftPair = rightPair = null;
            return;
        }
        leftPair = twoPick.Take(2).ToArray();
        rightPair = twoPick.TakeLast(2).ToArray();
    }

    private List<Card> GetTwoPickStorageByRound() {
        var format = (GameFormat)Player.currentDeck.format;
        var craft = (CardCraft)Player.currentDeck.craft;
        var craftFilter = (craft == CardCraft.Neutral) ? new Func<Card, bool>(card => card.Craft != CardCraft.Neutral)
            : new Func<Card, bool>(card => card.Craft == craft);

        if (format == GameFormat.TwoPick) {
            var legendRound = new List<int>(){ 0, 7, 14 };
            var bronzeRound = new List<int>(){ 1, 3, 6, 8, 10, 12 };
            var silverRound = new List<int>(){ 2, 5, 11, 13 };
            var neutralRound = new List<int>(){ 4, 9 };

            if (legendRound.Contains(Round))
                return twoPickStorage.Where(x => ((x.Craft == CardCraft.Neutral) && (x.Rarity == CardRarity.Legend)) || 
                    (craftFilter.Invoke(x) && (x.Rarity >= CardRarity.Gold))).ToList();
            
            if (bronzeRound.Contains(Round))
                return twoPickStorage.Where(x => craftFilter.Invoke(x) &&
                    (x.Rarity == CardRarity.Bronze)).ToList();

            if (silverRound.Contains(Round))
                return twoPickStorage.Where(x => craftFilter.Invoke(x) &&
                    (x.Rarity == CardRarity.Silver)).ToList();

            if (neutralRound.Contains(Round))
                return twoPickStorage.Where(x => (x.Craft == CardCraft.Neutral) &&
                    (x.Rarity >= CardRarity.Bronze) && (x.Rarity <= CardRarity.Gold)).ToList();

            return null;
        }
        
        return null;
    }

    public void Choose(int index) {
        var pair = (index == 0) ? leftPair : rightPair;
        foreach (var card in pair)
            Player.currentDeck.AddCard(card);
    }

}
