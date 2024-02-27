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
            } else if (trimId.TryTrimStart("all.", out trimId)) {
                return trimId switch {
                    "count" => all.Count,
                    "isMe"  => (all.Count == me.Count) ? 1 : 0,
                    "isOp"  => (all.Count == op.Count) ? 1 : 0,
                    _       => float.MinValue,
                };
            } else if (trimId.TryTrimStart("me.", out trimId)) {
                return trimId switch {
                    "count" => me.Count,
                    _       => float.MinValue,
                };
            } else if (trimId.TryTrimStart("op.", out trimId)) {
                return trimId switch {
                    "count" => op.Count,
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
            var sourceEffectAllCards = (new List<BattleCard>(){ sourceEffect.source }).Concat(sourceEffect.invokeTarget);
            var effectCards = trimUnit switch {
                "source" => new List<BattleCard>(){ sourceEffect.source },
                "target" => sourceEffect.invokeTarget,
                "me"     => sourceEffectAllCards.Where(x => state.GetBelongUnit(x).id == invokeUnit.id).ToList(),
                "op"     => sourceEffectAllCards.Where(x => state.GetBelongUnit(x).id == rhsUnit.id).ToList(),
                _ => new List<BattleCard>(),
            };

            if (!info.places.Contains(BattlePlaceId.None))
                effectCards.RemoveAll(x => !info.places.Contains(state.GetBelongUnit(x).GetBelongPlace(x).PlaceId));

            invokeTarget = info.mode[0] switch {
                "all"       => effectCards,
                "random"    => effectCards.Random(info.num, false),
                "first"     => effectCards.Take(info.num).ToList(),
                _           => invokeTarget,
            };

            return;
        }

        var allUnit = info.unit switch {
            "all"   =>  new List<BattleUnit>() { invokeUnit, rhsUnit },
            "me"    =>  new List<BattleUnit>() { invokeUnit },
            "op"    =>  new List<BattleUnit>() { rhsUnit },
            _       =>  new List<BattleUnit>(),
        };

        var allPlace = new List<BattlePlace>();
        for (int i = 0; i < info.places.Count; i++)
            allUnit.ForEach(unit => allPlace.Add(unit.GetPlace(info.places[i])));

        var allCards = new List<BattleCard>();
        allPlace.ForEach(x => allCards.AddRange(x.cards.Where(info.filter.FilterWithCurrentCard)));

        if (info.mode.Contains("other"))
            allCards.RemoveAll(x => x == source);

        switch (info.mode[0]) {
            default:
                break;
            case "all":
                invokeTarget = allCards;
                break;
            case "random":
                invokeTarget = allCards.Random(info.num, false);
                break;
            case "first":
                invokeTarget = allCards.Take(info.num).ToList();
                break;
            case "index":
                invokeTarget = new List<BattleCard>();
                for (int i = 0; i <= info.num; i++) {
                    var target = invokeUnit.targetQueue.Dequeue();
                    if (target == null)
                        break;

                    if (allCards.Contains(target))
                        invokeTarget.Add(target);
                }
                break;
        }
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