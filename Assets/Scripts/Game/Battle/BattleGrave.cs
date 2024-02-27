using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BattleGrave : BattlePlace
{
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

    public List<Card> GetFilterCards(Func<BattleCard, bool> filter) {
        return cards.Where(filter).Select(x => x.baseCard).ToList();
    }

    public List<Card> GetDistinctCards(List<Card> cards) {
        return cards.Select(x => x.id).Distinct().Select(Card.Get).OrderBy(CardDatabase.Sorter).ToList();
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
