using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leader
{
    public static string[] infoKeys => new string[] { "rally" };
    public static string[] infoValues => new string[] { "協作數" };

    public int CraftId => leaderCard.CurrentCard.CraftId;
    public CardCraft Craft => leaderCard.CurrentCard.Craft;
    public BattleCard leaderCard;
    public Dictionary<string, float> options = new Dictionary<string, float>();

    public int HpInit => Card.GetLeaderCard(CraftId).hp;
    public int Hp => leaderCard.CurrentCard.hp;
    public int HpMax => leaderCard.CurrentCard.hpMax;

    private int pp, ep;
    public int PPMax, EpMax;
    public int PP {
        get => pp;
        set => pp = Mathf.Clamp(value, 0, PPMax);
    }
    public int EP {
        get => ep;
        set => ep = Mathf.Clamp(value, 0, EpMax);
    }
    public bool isEpUsed;

    public Leader(bool isFirst, int craftId) {
        leaderCard = BattleCard.Get(Card.GetLeaderCard(craftId));
        ep = pp = PPMax = 0;
        EpMax = isFirst ? 2 : 3;
        isEpUsed = false;
    }

    public Leader(Leader rhs) {
        leaderCard = new BattleCard(rhs.leaderCard);
        options = new Dictionary<string, float>(rhs.options);
        pp = rhs.pp;
        PPMax = rhs.PPMax;
        ep = rhs.ep;
        EpMax = rhs.EpMax;
        isEpUsed = rhs.isEpUsed;
    }

    public void ClearTurnIdentifier() {
        SetIdentifier("combo", 0);

        foreach (var entry in options) {
            if (!entry.Key.StartsWith("turn"))
                continue;

            SetIdentifier(entry.Key, 0);
        }
    }

    public float GetIdentifier(string id) 
    {
        return id switch {
            "hp" => Hp,
            "hpMax" => HpMax,
            "hpInit" => HpInit,
            "pp" => pp,
            "ppMax" => PPMax,
            "ep" => ep,
            "epMax" => EpMax,
            "isEpUsed" => isEpUsed ? 1 : 0,
            _ => options.Get(id, 0),
        };
    }

    public void SetIdentifier(string id, float num) {
        switch (id) {
            default:
                options.Set(id, num);
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
                EpMax = (int)num;
                return;
            case "isEpUsed":
                isEpUsed = num != 0;
                return;
        }
    }

    public void AddIdentifier(string id, float num) {
        SetIdentifier(id, GetIdentifier(id) + num);
    }
}
