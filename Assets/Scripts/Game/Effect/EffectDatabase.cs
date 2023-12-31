using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EffectDatabase {

    private static Dictionary<string, EffectTarget> targetConvDict = new Dictionary<string, EffectTarget>() {
        {"none", EffectTarget.None},
        {"self", EffectTarget.Self},
    };

    private static Dictionary<string, EffectCondition> condConvDict = new Dictionary<string, EffectCondition>() {
        {"none", EffectCondition.None},
    };

    private static Dictionary<string, EffectAbility> abilityConvDict = new Dictionary<string, EffectAbility>() {
        {"none", EffectAbility.None},
        {"result", EffectAbility.SetResult},
        {"keep", EffectAbility.KeepCard},
        {"turn_start", EffectAbility.TurnStart},
        {"turn_end", EffectAbility.TurnEnd},
        {"use", EffectAbility.Use},
        {"cover", EffectAbility.Cover},
        {"attack", EffectAbility.Attack},
        {"evolve", EffectAbility.Evolve},
        {"fusion", EffectAbility.Fusion},
        {"act", EffectAbility.Act},

        {"draw", EffectAbility.Draw},
        {"summon", EffectAbility.Summon},
        {"set_keyword", EffectAbility.SetKeyword},
    };

    private static Dictionary<EffectTarget, string> targetNameDict = new Dictionary<EffectTarget, string>() {
        {EffectTarget.Self, "自己"},
    };

    public static EffectTarget ToEffectTarget(this string target) {
        return targetConvDict.Get(target, EffectTarget.None);
    }

    public static EffectCondition ToEffectCondition(this string condition) {
        return condConvDict.Get(condition, EffectCondition.None);
    }

    public static EffectAbility ToEffectAbility(this string ability) {
        return abilityConvDict.Get(ability, EffectAbility.None);
    }

    public static string GetEffectTargetName(this EffectTarget target) {
        return targetNameDict.Get(target, string.Empty);
    }
}

public enum EffectTarget {
    None,
    Self,
}

public enum EffectCondition {
    None,
}

public enum EffectAbility {
    None = 0,
    SetResult = 1,
    KeepCard = 2,
    TurnStart = 3,
    TurnEnd = 4,
    Use = 5,
    Cover = 6,
    Attack = 7,
    Evolve = 8,
    Fusion = 9,
    Act = 10,

    Draw = 101,
    Summon = 102,
    SetKeyword = 103,
}