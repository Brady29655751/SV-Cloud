using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Leader : BattlePlace
{
    public int CraftId => leaderCard.CurrentCard.CraftId;
    public CardCraft Craft => leaderCard.CurrentCard.Craft;
    public BattleCard leaderCard => cards[0];

    public int HPInit => Card.GetLeaderCard(CraftId).hp;
    public int HP => leaderCard.CurrentCard.hp;
    public int HPMax => leaderCard.CurrentCard.hpMax;

    private int pp, ppMax, ep, epMax;
    public int PPMax {
        get => ppMax;
        set => ppMax = Mathf.Clamp(value, 0, 10);
    }

    public int PP {
        get => pp;
        set => pp = Mathf.Clamp(value, 0, PPMax);
    }

    public int EPMax {
        get => epMax;
        set => epMax = Mathf.Clamp(value, 0, 3);
    }

    public int EP {
        get => ep;
        set => ep = Mathf.Clamp(value, 0, EPMax);
    }
    public bool isEpUsed;

    public Leader(bool isFirst, int craftId) : base(new List<BattleCard>() { BattleCard.Get(Card.GetLeaderCard(craftId)) }) {
        ep = pp = PPMax = 0;
        EPMax = isFirst ? 2 : 3;
        isEpUsed = false;
    }

    public Leader(Leader rhs) : base(rhs) {
        pp = rhs.pp;
        PPMax = rhs.PPMax;
        ep = rhs.ep;
        EPMax = rhs.EPMax;
        isEpUsed = rhs.isEpUsed;
    }

    public override BattlePlaceId GetPlaceId()
    {
        return BattlePlaceId.Leader;
    }

    public void ClearTurnIdentifier() {
        isEpUsed = false;
        
        SetIdentifier("combo", 0);

        var turnKeys = new List<string>(options.Keys.Where(x => x.StartsWith("turn")));
        for (int i = 0; i < turnKeys.Count; i++)
            SetIdentifier(turnKeys[i], 0);
    }

    public override float GetIdentifier(string id) 
    {
        return id switch {
            "craft" => CraftId,
            "hp" => HP,
            "hpMax" => HPMax,
            "hpInit" => HPInit,
            "pp" => PP,
            "ppMax" => PPMax,
            "ep" => EP,
            "epMax" => EPMax,
            "isEpUsed" => isEpUsed ? 1 : 0,
            "isAwake"  => ((options.Get("lockAwake") <= 0) && ((PPMax >= 7) || (options.Get("forceAwake") > 0)))  ? 1 : 0,
            "isVenge"  => ((options.Get("lockVenge") <= 0) && ((HP <= 10) || (options.Get("forceVenge") > 0)))  ? 1 : 0,
            "isEager"  => ((options.Get("lockEager") <= 0) && ((options.Get("turn_draw_cards") >= 2) || (options.Get("forceEager") > 0)))  ? 1 : 0,
            "isChase"  => ((options.Get("lockChase") <= 0) && ((options.Get("turn_give_op_leader_damage") >= 1) || (options.Get("forceChase") > 0)))  ? 1 : 0,
            _ => base.GetIdentifier(id),
        };
    }

    public override void SetIdentifier(string id, float num) {
        switch (id) {
            default:
                base.SetIdentifier(id, num);
                return;
            case "pp":
                PP = (int)num;
                return;
            case "ppMax":
                PPMax = (int)num;
                return;
            case "ep":
                EP = (int)num;
                return;
            case "epMax":
                EPMax = (int)num;
                return;
            case "isEpUsed":
                isEpUsed = num != 0;
                return;
        }
    }

    public override void AddIdentifier(string id, float num) {
        SetIdentifier(id, GetIdentifier(id) + num);
    }
}
