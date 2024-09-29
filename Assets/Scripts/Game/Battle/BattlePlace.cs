using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattlePlace
{
    public BattlePlaceId PlaceId => GetPlaceId();
    public List<BattleCard> cards = new List<BattleCard>();
    public int MaxCount;
    public int Count => cards.Count;
    public int AvailableCount => MaxCount - Count;
    public bool IsFull => Count >= MaxCount;
    public Dictionary<string, int> options = new Dictionary<string, int>();

    public BattlePlace() {}
    public BattlePlace(List<BattleCard> battleCards) {
        cards = battleCards;
        MaxCount = cards.Count;
    }

    public BattlePlace(BattlePlace rhs) {
        cards = rhs.cards.Select(x => (x == null) ? null : new BattleCard(x)).ToList();
        MaxCount = rhs.MaxCount;
        options = new Dictionary<string, int>(rhs.options);
    }

    public virtual BattlePlaceId GetPlaceId() {
        return BattlePlaceId.None;
    }

    public virtual bool Contains(BattleCard battleCard) {
        return cards.Contains(battleCard);
    }

    public virtual int GetIdentifier(string id) {
        var trimId = id.Split('.')[0];

        if (id.StartsWith("[")) {
            var filter = BattleCardFilter.Parse(trimId);
            var filterCards = cards.Where(filter.FilterWithCurrentCard).ToList();

            trimId = id.TrimStart(trimId).TrimStart('.');
            if (trimId.TryTrimStart("first.", out trimId))
                return filterCards.FirstOrDefault()?.GetIdentifier(trimId) ?? 0;

            return trimId switch {
                "count" => filterCards.Count,
                _ => 0,
            };
        }

        return id switch {
            "count"         => Count,
            "maxCount"      => MaxCount,
            "availableCount"=> AvailableCount,
            "isFull"        => IsFull ? 1 : 0,
            _               => options.Get(id, 0),
        };
    }

    public virtual void SetIdentifier(string id, int num) {
        options.Set(id, num);
    }

    public virtual void AddIdentifier(string id, int num) {
        SetIdentifier(id, GetIdentifier(id) + num);
    }
}

