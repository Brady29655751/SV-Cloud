using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCardActionController
{
    public int StayFieldTurn {
        get => GetIdentifier("stayFieldTurn");
        set => SetIdentifier("stayFieldTurn", value);
    }

    public int MaxAttackChance {
        get => GetIdentifier("maxAttackChance");
        set => SetIdentifier("maxAttackChance", value); 
    }

    public int CurrentAttackChance {
        get => Mathf.Max(MaxAttackChance - GetIdentifier("usedAttackChance"), 0);
        set => SetIdentifier("usedAttackChance", MaxAttackChance - value);
    }

    public bool IsAttackFinished => CurrentAttackChance == 0;

    public List<KeyValuePair<Func<bool>, CardKeyword>> keywordList = new List<KeyValuePair<Func<bool>, CardKeyword>>();
    public List<KeyValuePair<Func<bool>, int>> maxAttackChanceList = new List<KeyValuePair<Func<bool>, int>>();
    public Dictionary<string, int> options = new Dictionary<string, int>();

    public BattleCardActionController(int maxAttackChance) {
        MaxAttackChance = maxAttackChance;
        CurrentAttackChance = MaxAttackChance;
    }

    public BattleCardActionController(BattleCardActionController rhs) {
        keywordList = new List<KeyValuePair<Func<bool>, CardKeyword>>(rhs.keywordList);
        maxAttackChanceList = new List<KeyValuePair<Func<bool>, int>>(rhs.maxAttackChanceList);
        options = new Dictionary<string, int>(rhs.options);
    }

    public int GetIdentifier(string id) 
    {
        var keyword = id.ToCardKeyword();
        if (keyword != CardKeyword.None)
            return keywordList.Count(x => x.Value == keyword);

        return id switch {
            "isAttackFinished" => IsAttackFinished ? 1 : 0,
            "maxAttackChance" => maxAttackChanceList.Max(x => x.Value),
            _ => options.Get(id, 0),
        };
    }

    public void SetIdentifier(string id, int num) {
        switch (id) {
            default:
                options.Set(id, num);
                return;
            case "maxAttackChance":
                maxAttackChanceList.Add(new KeyValuePair<Func<bool>, int>(null, num));
                return;
        }
    }

    public void AddIdentifier(string id, int num) {
        SetIdentifier(id, GetIdentifier(id) + num);
    }

    public void RemoveUntilEffect() {
        keywordList.RemoveAll(x => x.Key?.Invoke() ?? false);
        maxAttackChanceList.RemoveAll(x => x.Key?.Invoke() ?? false);
    }

    public void OnTurnStartInField() {
        StayFieldTurn += 1;
        CurrentAttackChance = MaxAttackChance;
    }

    public bool IsKeywordAvailable(CardKeyword keyword) {
        return keywordList.Count(x => x.Value == keyword) > 0;
    }

    public void SetKeyword(Func<bool> untilFunc, CardKeyword keyword) {
        keywordList.Add(new KeyValuePair<Func<bool>, CardKeyword>(untilFunc, keyword));
    }

    public void RemoveKeyword(CardKeyword keyword) {
        keywordList.RemoveAll(x => x.Value == keyword);
    }
}
