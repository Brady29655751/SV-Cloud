using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleCardBuffController
{
    private int costBuff, atkBuff, hpBuff, damage;
    public List<KeyValuePair<Func<bool>, CardStatus>> tmpBuff = new List<KeyValuePair<Func<bool>, CardStatus>>();

    // tmpSetBuff does not involve in set-cost action.
    public List<KeyValuePair<Func<bool>, CardStatus>> tmpSetBuff = new List<KeyValuePair<Func<bool>, CardStatus>>();
    public Dictionary<string, int> options = new Dictionary<string, int>();

    public int CostBuff => costBuff + tmpBuff.Sum(x => x.Value.cost);
    public int AtkBuff => atkBuff + tmpBuff.Sum(x => x.Value.atk);
    public int HpBuff => hpBuff + tmpBuff.Sum(x => x.Value.hp);

    public int AtkSet => tmpSetBuff.Exists(x => x.Value.atk >= 0) ?
            tmpSetBuff.FindLast(x => x.Value.atk >= 0).Value.atk : -1;
    public int HpSet => tmpSetBuff.Exists(x => x.Value.hp >= 0) ?
            tmpSetBuff.FindLast(x => x.Value.hp >= 0).Value.hp : -1;

    public int Damage {
        get => damage;
        set => damage = value;
    }

    public BattleCardBuffController() {
        costBuff = atkBuff = hpBuff = damage = 0;
        tmpBuff = new List<KeyValuePair<Func<bool>, CardStatus>>();
        tmpSetBuff = new List<KeyValuePair<Func<bool>, CardStatus>>();
        options = new Dictionary<string, int>();
    }

    public BattleCardBuffController(BattleCardBuffController rhs) {
        costBuff = rhs.costBuff;
        atkBuff = rhs.atkBuff;
        hpBuff = rhs.hpBuff;
        damage = rhs.damage;
        tmpBuff = rhs.tmpBuff.ToList();
        tmpSetBuff = rhs.tmpSetBuff.ToList();
        options = new Dictionary<string, int>(rhs.options);
    }

    public int GetIdentifier(string id) {
        return id switch {
            "damage" => Damage,
            "isBuffed" => ((AtkBuff > 0) || (HpBuff > 0)) ? 1 : 0,
            "isDebuffed" => ((AtkBuff < 0) || (HpBuff < 0)) ? 1 : 0,
            _ => options.Get(id, 0),
        };
    }

    public void SetIdentifier(string id, int num) {
        options.Set(id, num);
    }

    public void RemoveUntilEffect() {
        tmpBuff.RemoveAll(x => x.Key.Invoke());
        tmpSetBuff.RemoveAll(x => x.Key?.Invoke() ?? false);
    }

    public int GetBuffedAtk(int atk) {
        var newAtk = (AtkSet >= 0) ? AtkSet : atk;
        return newAtk + AtkBuff;
    }

    public int GetBuffedHp(int hp) {
        var newHp = (HpSet >= 0) ? HpSet : hp;
        return newHp + HpBuff;
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

    public void SetBuff(CardStatus status, Func<bool> untilCondition) {
        if (status.atk >= 0)
            ClearAtkBuff();

        if (status.hp >= 0) {
            damage = 0;
            ClearHpBuff();
        }
        
        tmpSetBuff.Add(new KeyValuePair<Func<bool>, CardStatus>(untilCondition, status));
    }
    
    public void ClearCostBuff() {
        costBuff = 0;
        tmpBuff.RemoveAll(x => x.Value.cost != 0);
    }

    public void ClearAtkBuff() {
        atkBuff = 0;
        var allIndex = tmpBuff.FindAllIndex(x => x.Value.atk != 0);
        for (int i = 0; i < allIndex.Length; i++) {
            var index = allIndex[i];
            var originalBuff = tmpBuff[index];
            var originalKey = originalBuff.Key;
            var originalValue = originalBuff.Value;
            tmpBuff[index] = new KeyValuePair<Func<bool>, CardStatus>(originalKey, new CardStatus(originalValue.cost, 0, originalValue.hp));
        }
        tmpBuff.RemoveAll(x => x.Value.IsEmpty());
    }

    public void ClearHpBuff() {
        hpBuff = 0;
        var allIndex = tmpBuff.FindAllIndex(x => x.Value.hp != 0);
        for (int i = 0; i < allIndex.Length; i++) {
            var index = allIndex[i];
            var originalBuff = tmpBuff[index];
            var originalKey = originalBuff.Key;
            var originalValue = originalBuff.Value;
            tmpBuff[index] = new KeyValuePair<Func<bool>, CardStatus>(originalKey, new CardStatus(originalValue.cost, originalValue.atk, 0));
        }
        tmpBuff.RemoveAll(x => x.Value.IsEmpty());
    }

}
