using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleField : BattlePlace
{
    public BattleField() {
        MaxCount = 5;
    }
    public BattleField(BattleField rhs) : base(rhs) {}

    public List<int> GetAttackableTargetIndex(BattleCard attackSource, BattleUnit sourceUnit) {
        var result = cards.Where(x => (x.CurrentCard.IsFollower()) || (x.CurrentCard.Type == CardType.Leader));
        var attackable = result.Where(x => (!x.actionController.IsKeywordAvailable(CardKeyword.Ambush)) && (!x.actionController.IsKeywordAvailable(CardKeyword.Pressure)));
        var ward = attackable.Where(x => x.actionController.IsKeywordAvailable(CardKeyword.Ward));

        result = ward.Any() ? ward : attackable;
        var index = result.Select(x => cards.IndexOf(x)).ToList();

        if ((attackSource.IsLeaderAttackable(sourceUnit)) && (!ward.Any()))
            index.Add(-1);

        return index;
    }

    public override BattlePlaceId GetPlaceId()
    {
        return BattlePlaceId.Field;
    }

    public override int GetIdentifier(string id)
    {
        switch (id) {
            default:
                return base.GetIdentifier(id);
            case "isUnion":
                return (cards.Exists(x => x.CurrentCard.traits.Contains(CardTrait.Commander)) && 
                    cards.Exists(x => x.CurrentCard.traits.Contains(CardTrait.Soldier))) ? 1 : 0;
        }
    }

    public override void SetIdentifier(string id, int num)
    {
        base.SetIdentifier(id, num);
    }

    public override void AddIdentifier(string id, int num)
    {
        SetIdentifier(id, GetIdentifier(id) + num);
    }
}
