using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleDeck : BattlePlace
{
    public int zone, format, craft;
    
    public BattleDeck(int zoneId, int formatId, int craftId, int[] cardIds) : base(cardIds.Select(x => BattleCard.Get(Card.Get(x))).ToList()) {
        zone = zoneId;
        format = formatId;
        craft = craftId;
        
        cards.Shuffle();
    }

    public BattleDeck(BattleDeck rhs) : base(rhs) {
        zone = rhs.zone;
        format = rhs.format;
        craft = rhs.craft;
    }

    public override BattlePlaceId GetPlaceId()
    {
        return BattlePlaceId.Deck;
    }

    public override int GetIdentifier(string id) {
        return id switch {
            "zone"  => zone,
            "format"=> format,
            "craft" => craft,
            _       => base.GetIdentifier(id)
        };
    }

    public override void SetIdentifier(string id, int num) {
        base.SetIdentifier(id, num);
    }

    public override void AddIdentifier(string id, int num) {
        SetIdentifier(id, GetIdentifier(id) + num);
    }
}
