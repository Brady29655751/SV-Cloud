using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattlePlace
{
    public BattlePlaceId placeId => GetPlaceId();
    public List<BattleCard> cards = new List<BattleCard>();
    public int MaxCount;
    public int Count => cards.Count;
    public bool IsFull => Count >= MaxCount;
    public Dictionary<string, float> options = new Dictionary<string, float>();

    public BattlePlace() {}
    public BattlePlace(List<BattleCard> battleCards) {
        cards = battleCards;
        MaxCount = cards.Count;
    }

    public BattlePlace(BattlePlace rhs) {
        cards = rhs.cards.Select(x => (x == null) ? null : new BattleCard(x)).ToList();
        MaxCount = rhs.MaxCount;
        options = new Dictionary<string, float>(rhs.options);
    }

    protected virtual BattlePlaceId GetPlaceId() {
        return BattlePlaceId.None;
    }

    public virtual bool Contains(BattleCard battleCard) {
        return cards.Contains(battleCard);
    }

    public virtual float GetIdentifier(string id) {
        return options.Get(id, 0);
    }

    public virtual void SetIdentifier(string id, float num) {
        options.Set(id, num);
    }

    public virtual void AddIdentifier(string id, float num) {
        SetIdentifier(id, GetIdentifier(id) + num);
    }
}
