using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class BattleGrave : BattlePlace
{
    public static string[] CardListNames = new string[] { "usedCards" };
    public int GraveCount = 0;
    public List<Card> usedCards = new List<Card>();
    public List<Card> DestroyedCards => GetFilterCards(x => x.GetIdentifier("graveReason") == (float)BattleCardGraveReason.Destroy);
    public List<Card> DestroyedFollowers => DestroyedCards.Where(x => x.IsFollower()).ToList();
    public List<Card> DestroyedAmulets => DestroyedCards.Where(x => x.Type == CardType.Amulet).ToList();

    public List<Card> DistinctUsedCards => GetDistinctCards(usedCards);
    public List<Card> DistinctDestroyedCards => GetDistinctCards(DestroyedCards);
    public List<Card> DistinctDestroyedFollowers => GetDistinctCards(DestroyedFollowers);
    public List<Card> DistinctDestroyedAmulets => GetDistinctCards(DestroyedAmulets);

    public BattleGrave() {
        MaxCount = 999;
    }
    public BattleGrave(BattleGrave rhs) : base(rhs) {
        GraveCount = rhs.GraveCount;
        usedCards = rhs.usedCards.Select(x => (x == null) ? null : new Card(x)).ToList();
    }

    public override BattlePlaceId GetPlaceId()
    {
        return BattlePlaceId.Grave;
    }

    public override bool Contains(BattleCard battleCard)
    {
        return base.Contains(battleCard) || usedCards.Contains(battleCard.baseCard);
    }

    public override int GetIdentifier(string id) 
    {
        for (int i = 0; i < BattleGrave.CardListNames.Length; i++) {
            var listName = BattleGrave.CardListNames[i];
            if (id.TryTrimStart(listName, out var trimId)) {
                var cardList = GetCardListByName(listName);
                if (trimId.StartsWith("[")) {
                    var endIndex = trimId.IndexOf("].");
                    var prefix = trimId.Substring(0, endIndex + 1);
                    var filter = CardFilter.Parse(prefix, (filterType, paramToSet) => 
                            Parser.ParseEffectExpression(paramToSet, Player.currentBattle.CurrentState.currentEffect, Player.currentBattle.CurrentState).ToString());
                    if (filter.options.TryGetValue("usedTurn", out var usedTurn))
                        filter.options.Set("usedTurn", turn - usedTurn);
                    
                    cardList = cardList.Where(filter.Filter).Where(filter.OptionFilter).ToList();
                    trimId = trimId.TrimStart(prefix).TrimStart('.');
                }
                if (trimId.TryTrimStart("first.", out trimId))
                    return cardList.FirstOrDefault()?.GetIdentifier(trimId) ?? 0;
                    
                return trimId switch {
                    "count" => cardList.Count,
                    _ => 0,
                };
            }
        }

        return id switch {
            "num" => GraveCount,
            "distinct_destroyedFollowerCostCount" => DistinctDestroyedFollowers.Select(x => x.cost).Distinct().Count(),
            _ => base.GetIdentifier(id),
        };
    }

    public List<Card> GetFilterCards(Func<BattleCard, bool> filter) {
        return cards.Where(filter).Select(x => x.baseCard).ToList();
    }

    public List<Card> GetDistinctCards(List<Card> cards) {
        return cards.Select(x => x.id).Distinct().Select(Card.Get).OrderBy(CardDatabase.Sorter).ToList();
    }

    public List<Card> GetCardListByName(string name) {
        return name switch {
            "usedCards"     =>  usedCards,
            _               =>  cards.Select(x => x.CurrentCard).ToList(),
        };
    }
}

public enum BattleCardGraveReason {
    None = 0,
    DrawTooMuch = 1,
    Destroy = 2,
    Vanish = 3,
    Return = 4,
    Discard = 5,
}
