using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCardActionController
{
    public int StayFieldTurn {
        get => (int)GetIdentifier("stayFieldTurn");
        set => SetIdentifier("stayFieldTurn", value);
    }

    public int MaxAttackChance {
        get => (int)GetIdentifier("maxAttackChance");
        set => SetIdentifier("maxAttackChance", value); 
    }

    public int CurrentAttackChance {
        get => Mathf.Clamp((int)GetIdentifier("attackChance"), 0, MaxAttackChance);
        set => SetIdentifier("attackChance", Mathf.Clamp(value, 0, MaxAttackChance));
    }

    public bool IsAttackFinished => CurrentAttackChance == 0;

    public Dictionary<string, float> options = new Dictionary<string, float>();

    public BattleCardActionController() {
        MaxAttackChance = 1;
        CurrentAttackChance = 1;
    }

    public BattleCardActionController(BattleCardActionController rhs) {
        options = new Dictionary<string, float>(rhs.options);
    }

    public float GetIdentifier(string id) 
    {
        return id switch {
            "isAttackFinished" => IsAttackFinished ? 1 : 0,
            _ => options.Get(id, 0),
        };
    }

    public void SetIdentifier(string id, float num) {
        switch (id) {
            default:
                options.Set(id, num);
                return;
        }
    }

    public void AddIdentifier(string id, float num) {
        SetIdentifier(id, GetIdentifier(id) + num);
    }

    public void OnTurnStartInField() {
        StayFieldTurn += 1;
        CurrentAttackChance = MaxAttackChance;
    }

    public bool IsKeywordAvailable(CardKeyword keyword) {
        return GetIdentifier(keyword.GetKeywordEnglishName()) > 0;
    }
}
