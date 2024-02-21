using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGrave : BattlePlace
{
    public int GraveCount = 0;
    public List<Card> usedCards = new List<Card>();
    public List<Card> destroyedCards => cards.Where(x => x.GetIdentifier("graveReason") == (float)BattleCardGraveReason.Destroy).Select(x => x.baseCard).ToList();
    public List<Card> destroyedFollowers => destroyedCards.Where(x => x.IsFollower()).ToList();

    public List<Card> DistinctUsedCards => usedCards.Select(x => x.id).Distinct().Select(Card.Get).OrderBy(CardDatabase.Sorter).ToList();
    public List<Card> DistinctDestroyedFollowers => destroyedFollowers.Select(x => x.id).Distinct().Select(Card.Get).OrderBy(CardDatabase.Sorter).ToList();

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
}

public enum BattleCardGraveReason {
    None = 0,
    DrawTooMuch = 1,
    Destroy = 2,
    Vanish = 3,
    Return = 4,
    Discard = 5,
}
