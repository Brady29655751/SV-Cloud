using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleCardBuffController
{
    private int costBuff, atkBuff, hpBuff, damage;
    public List<KeyValuePair<Func<bool>, CardStatus>> tmpBuff = new List<KeyValuePair<Func<bool>, CardStatus>>();
    public Dictionary<string, int> options = new Dictionary<string, int>();

    public int CostBuff => costBuff + tmpBuff.Sum(x => x.Value.cost);
    public int AtkBuff => atkBuff + tmpBuff.Sum(x => x.Value.atk);
    public int HpBuff => hpBuff + tmpBuff.Sum(x => x.Value.hp);
    public int Damage {
        get => damage;
        set => damage = value;
    }

    public BattleCardBuffController() {
        costBuff = atkBuff = hpBuff = damage = 0;
        tmpBuff = new List<KeyValuePair<Func<bool>, CardStatus>>();
        options = new Dictionary<string, int>();
    }

    public BattleCardBuffController(BattleCardBuffController rhs) {
        costBuff = rhs.costBuff;
        atkBuff = rhs.atkBuff;
        hpBuff = rhs.hpBuff;
        damage = rhs.damage;
        tmpBuff = rhs.tmpBuff.ToList();
        options = new Dictionary<string, int>(rhs.options);
    }

    public int GetIdentifier(string id) {
        return id switch {
            "isBuffed" => ((AtkBuff > 0) || (HpBuff > 0)) ? 1 : 0,
            _ => options.Get(id, 0),
        };
    }

    public void SetIdentifier(string id, int num) {
        options.Set(id, num);
    }

    public void RemoveUntilEffect() {
        tmpBuff.RemoveAll(x => x.Key.Invoke());
    }

    public int TakeDamage(int dmg) {
        damage += Mathf.Max(dmg, 0);
        return dmg;
    }

    public int TakeHeal(int heal) {
        var realHeal = Mathf.Min(heal, damage);
        damage -= realHeal;
        return realHeal;
    }

    public void TakeBuff(CardStatus status, Func<bool> untilCondition) {
        if (untilCondition == null) {
            costBuff += status.cost;
            atkBuff += status.atk;
            hpBuff += status.hp;
            return;
        }
        tmpBuff.Add(new KeyValuePair<Func<bool>, CardStatus>(untilCondition, status));
    }
    
    public void ClearCostBuff() {
        costBuff = 0;
        tmpBuff.RemoveAll(x => x.Value.cost != 0);
    }

}
