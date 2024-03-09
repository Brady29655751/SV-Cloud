using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : IIdentifyHandler
{
    public const int DATA_COL = 7;
    public static Effect Get(int id) {
        var effect = DatabaseManager.instance.GetEffectInfo(id);
        return (effect == null) ? null : new Effect(effect);
    }
    public static Effect None => new Effect("none", "none", null, null, EffectAbility.None, null);
    public static Battle Battle => Player.currentBattle;

    public BattleCard source = null;
    public Effect sourceEffect = null;
    public BattlePlace sourcePlace = null;
    public List<BattleCard> invokeTarget = null;
    public BattleUnit invokeUnit = null;

    public int id;
    public int Id => id;
    public string timing { get; private set; }
    public string target { get; private set; }
    public List<string> condition { get; private set; }
    public List<List<ICondition>> condOptionDictList { get; private set; } = new List<List<ICondition>>();
    public EffectAbility ability { get; private set; }
    public Dictionary<string, string> abilityOptionDict { get; private set; } = new Dictionary<string, string>();
    public Dictionary<string, string> hudOptionDict { get; private set; } = new Dictionary<string, string>();

    public Effect(string[] _data, int startIndex) {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);

        source = null;
        id = int.Parse(_slicedData[0]);
        timing = _slicedData[1];
        target = _slicedData[2];
        condition = (_slicedData[3] == "none") ? null : _slicedData[3].Split('/').ToList();
        condOptionDictList.ParseMultipleCondition(_slicedData[4]);
        ability = _slicedData[5].ToEffectAbility();
        abilityOptionDict.ParseOptions(_slicedData[6]);
        hudOptionDict = new Dictionary<string, string>();
    }

    public Effect(string _timing, string _target, 
        List<string> _condition, List<List<ICondition>> _condition_option,
        EffectAbility _ability, Dictionary<string, string> _ability_option) {
        source = null;
        timing = _timing;
        target = _target;
        condition = _condition;
        if (_condition_option == null) {
            condOptionDictList.Add(new List<ICondition>());
        } else {
            condOptionDictList = _condition_option;
        }
        ability = _ability;
        abilityOptionDict = _ability_option ?? new Dictionary<string, string>();
        hudOptionDict = new Dictionary<string, string>();
    }

    public Effect(int[] data) {
        source = null;
        timing = "none";
        target = "none";
        condition = null;
        condOptionDictList.Add(new List<ICondition>());
        ability = (EffectAbility)data[0];
        abilityOptionDict = Parse(ability, data.SubArray(1));
        hudOptionDict = new Dictionary<string, string>();
    }

    public Effect(Effect rhs) {
        id = rhs.id;
        timing = rhs.timing;
        target = rhs.target;
        condition = rhs.condition;
        ability = rhs.ability;
        condOptionDictList = rhs.condOptionDictList.Select(x => x.Select(y => new ICondition(y.op, y.lhs, y.rhs)).ToList()).ToList();
        abilityOptionDict = new Dictionary<string, string>(rhs.abilityOptionDict);
        hudOptionDict = new Dictionary<string, string>(rhs.hudOptionDict);

        source = rhs.source;
        sourceEffect = rhs.sourceEffect;
        invokeUnit = rhs.invokeUnit;
        invokeTarget = rhs.invokeTarget?.ToList();
    }

    public bool TryGetIdenfier(string id, out float value)
    {
        value = GetIdentifier(id);
        return value != float.MinValue;
    }

    public float GetIdentifier(string id)
    {
        var state = Player.currentBattle.CurrentState;
        var trimId = string.Empty;

        if (id.TryTrimStart("source.", out trimId)) {
            return trimId switch {
                "isMe"  => (invokeUnit.GetBelongPlace(source) != null) ? 1 : 0,
                "where" => (float)(invokeUnit.GetBelongPlace(source)?.PlaceId ?? BattlePlaceId.None),
                _ => source.GetIdentifier(trimId),
            };
        }

        if (id.TryTrimStart("ability", out trimId)) {
            if (trimId.TryTrimParentheses(out var abilityType))
                return float.Parse(abilityOptionDict.Get(abilityType, "0"));
        }

        return id switch {
            _ => float.MinValue,
        };
    }

    public float GetSourceEffectIdentifier(string id, BattleState state) {
        var trimId = string.Empty;

        if (id.TryTrimStart("target.", out trimId)) {
            var all = sourceEffect.invokeTarget;
            var me = all.Where(x => state.GetBelongUnit(x).id == invokeUnit.id).ToList();
            var op = all.Where(x => state.GetBelongUnit(x).id == state.GetRhsUnitById(invokeUnit.id).id).ToList();

            if (trimId.TryTrimStart("first.", out trimId)) {
                var card = all.FirstOrDefault();
                if (card == null)
                    return 0;

                return trimId switch {
                    "isMe"  => me.Contains(card) ? 1 : 0,
                    "isOp"  => op.Contains(card) ? 1 : 0,
                    "where" => (float)(invokeUnit.GetBelongPlace(source)?.PlaceId ?? BattlePlaceId.None),
                    _       => card.GetIdentifier(trimId),
                };
            } else {
                var prefix = trimId.Split('.')[0];
                var trimPrefix = string.Empty;
                var cards = all;

                if (prefix.TryTrimStart("me", out trimPrefix))
                    cards = me;
                else if (prefix.TryTrimStart("op", out trimPrefix))
                    cards = op;
                else    
                    trimPrefix = prefix.TrimStart("all");

                if (trimPrefix.StartsWith("[")) {
                    var filter = BattleCardFilter.Parse(trimPrefix);
                    cards = cards.Where(filter.FilterWithCurrentCard).ToList();
                } 

                trimId = trimId.TrimStart(prefix).TrimStart('.');

                return trimId switch {
                    "count" => cards.Count,
                    "isMe"  => (cards.Count == me.Count) ? 1 : 0,
                    "isOp"  => (cards.Count == op.Count) ? 1 : 0,
                    _       => float.MinValue,
                };
            } 
        } else if (id.TryTrimStart("source.", out trimId)) {
            var all = sourceEffect.source;

            return trimId switch {
                "isMe"  => (invokeUnit.GetBelongPlace(all) != null) ? 1 : 0,
                "where" => (float)(invokeUnit.GetBelongPlace(all)?.PlaceId ?? BattlePlaceId.None),
                _ => source.GetIdentifier(trimId),
            };
        }

        return sourceEffect.GetIdentifier(id);
    }

    public void SetIdentifier(string id, float value)
    {
        var trimId = string.Empty;

        if (id.TryTrimStart("source.", out trimId))
            source?.SetIdentifier(trimId, value); 

        else if (id.TryTrimStart("target.", out trimId)) {
            var all = invokeTarget;
            var me = all.Where(x => Battle.CurrentState.GetBelongUnit(x).id == invokeUnit.id).ToList();
            var op = all.Where(x => Battle.CurrentState.GetBelongUnit(x).id == Battle.CurrentState.GetRhsUnitById(invokeUnit.id).id).ToList();

            var prefix = trimId.Split('.')[0];
            var trimPrefix = string.Empty;
            var cards = all;

            if (prefix.TryTrimStart("me", out trimPrefix))
                cards = me;
            else if (prefix.TryTrimStart("op", out trimPrefix))
                cards = op;
            else    
                trimPrefix = prefix.TrimStart("all");

            if (trimPrefix.StartsWith("[")) {
                var filter = BattleCardFilter.Parse(trimPrefix);
                cards = cards.Where(filter.FilterWithCurrentCard).ToList();
            } 

            trimId = trimId.TrimStart(prefix).TrimStart('.');
            cards.ForEach(x => x.SetIdentifier(trimId, value));
        }
    }

    public EffectTargetInfo GetEffectTargetInfo(BattleState state) {
        return EffectTargetInfo.Parse(this, state);
    }

    public Dictionary<string, string> Parse(EffectAbility action, int[] data) {
        return EffectParseHandler.GetParseFunc(action).Invoke(data);
    }

    public bool Condition(BattleState state) {
        return condOptionDictList.Exists(each => each.All(
            cond => Operator.Condition(cond.op, 
                Parser.ParseEffectExpression(cond.lhs, this, state),
                Parser.ParseEffectExpression(cond.rhs, this, state)
            )
        ));
    }
    
    public bool Apply(BattleState state = null) {
        
        state.currentEffect = this;

        var result = true;
        var repeat = Parser.ParseEffectExpression(abilityOptionDict.Get("repeat", "1"), this, state);
        var abilityFunc = EffectAbilityHandler.GetAbilityFunc(ability);

        for (int i = 0; i < repeat; i++) {
            if (!EffectAbilityHandler.Preprocess(this, state)) {
                result = false;
                continue;
            }
            result &= abilityFunc.Invoke(this, state);
        }

        if (result)
            EffectAbilityHandler.Postprocess(this, state);
        
        return result;
    }

    public void SetInvokeTarget(BattleState state) {
        var rhsUnit = state.GetRhsUnitById(invokeUnit.id);
        var info = GetEffectTargetInfo(state);

        if (info.unit == "none")
            return;

        if (info.unit == "self") {
            invokeTarget = new List<BattleCard>() { source };
            return;
        } 

        if (info.unit.TryTrimStart("sourceEffect.", out var trimUnit)) {
            invokeTarget = info.GetSourceEffectTarget(this, state);                                                                                                                                                                        
            return;
        }

        invokeTarget = info.GetTarget(this, state);
    }

    public Func<bool> GetCheckCondition(string checkTiming, BattleState state) {
        return checkTiming switch {
            "turn_end"      => () => state.currentEffect.ability == EffectAbility.TurnEnd,
            "me_turn_end"   => () => (state.currentEffect.ability == EffectAbility.TurnEnd) && (invokeUnit.isDone),
            "op_turn_end"   => () => (state.currentEffect.ability == EffectAbility.TurnEnd) && (state.GetRhsUnitById(invokeUnit.id).isDone),
            _               => null,
        };
    }
}