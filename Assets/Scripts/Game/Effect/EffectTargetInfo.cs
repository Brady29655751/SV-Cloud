using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EffectTargetInfo  {
    public Effect effect;
    public string unit;
    public List<BattlePlaceId> places;
    public int num;
    public List<string> mode;
    public BattleCardFilter filter = new BattleCardFilter(-1);
    public List<string> options;

    public static EffectTargetInfo Parse(Effect effect, BattleState state) {
        var info = new EffectTargetInfo();
        var options = effect.target.Split('_');

        info.effect = effect;
        info.unit = options[0];
        if ((info.unit == "none") || (info.unit == "self"))
            return info;

        info.places = options[1].Split('+').Select(x => x.ToBattlePlace()).ToList();
        info.num = Parser.ParseEffectExpression(options[2], effect, state);
        info.mode = options[3].Split('+').ToList();

        if (options.Length <= 4)
            return info;

        info.filter = BattleCardFilter.Parse(options[4]);
        info.options = options.SubArray(5).ToList();
        return info;
    }

    private List<BattleCard> GetIndexTarget(List<BattleCard> allCards, BattleUnit invokeUnit) {
        var indexTarget = new List<BattleCard>();
        for (int i = 0; i <= num; i++) {
            var target = invokeUnit.targetQueue.Dequeue();
            if (target == null)
                break;

            if (allCards.Contains(target) || places.Contains(BattlePlaceId.Token))
                indexTarget.Add(target);
        }
        return indexTarget;
    }

    private List<BattleCard> ExcludeTargetByMode(List<BattleCard> allCards, Effect effect) {
        var source = effect.source;
        var sourceEffect = effect.sourceEffect;

        var excludeResult = allCards.ToList();

        for (int i = 0; i < mode.Count; i++) {
            if (!mode[i].TryTrimStart("other", out var trimMode))
                continue;

            if (trimMode == string.Empty)
                trimMode += "[source]";

            while (trimMode.TryTrimParentheses(out var excludeType)) {
                Predicate<BattleCard> predicate = excludeType switch {
                    "source"              => x => x == source,
                    "sourceEffect.target" => sourceEffect.invokeTarget.Contains,
                    _ => x => false,
                };
                excludeResult.RemoveAll(predicate);
                trimMode = trimMode.TrimStart("[" + excludeType + "]");
            }
        }
        return excludeResult;
    }

    private List<BattleCard> SortTargetByMode(List<BattleCard> allCards, Effect effect) {
        var source = effect.source;
        var sourceEffect = effect.sourceEffect;

        if (allCards.Count == 0)
            return allCards;

        for (int i = 0; i < mode.Count; i++) {
            if (!mode[i].TryTrimStart("sort", out var trimMode))
                continue;

            if (trimMode.TryTrimParentheses(out var sortInfo)) {
                var splitInfo = sortInfo.Split(':');
                var sortType = ((splitInfo[0].Split('.').Length > 1) ? string.Empty : "current.") + splitInfo[0].TrimStart("options.");
                var sortOrder = splitInfo[1];

                var sortResult = (sortOrder == "max") ? allCards.OrderByDescending(x => x.GetIdentifier(sortType))
                    : allCards.OrderBy(x => x.GetIdentifier(sortType));

                return sortResult.Where((x, i) => x.GetIdentifier(sortType) == sortResult.First().GetIdentifier(sortType)).ToList();
            }
        }
        return allCards;
    }

    public List<BattleCard> GetSourceEffectTarget(Effect effect, BattleState state) {
        var invokeUnit = effect.invokeUnit;
        var rhsUnit = state.GetRhsUnitById(invokeUnit.id);

        var sourceEffect = effect.sourceEffect;
        var sourceEffectAllCards = (new List<BattleCard>(){ sourceEffect.source }).Concat(sourceEffect.invokeTarget);

        var effectCards = unit.TrimStart("sourceEffect.") switch {
            "source" => new List<BattleCard>(){ sourceEffect.source },
            "target" => sourceEffect.invokeTarget,
            "me"     => sourceEffectAllCards.Where(x => state.GetBelongUnit(x).id == invokeUnit.id).ToList(),
            "op"     => sourceEffectAllCards.Where(x => state.GetBelongUnit(x).id == rhsUnit.id).ToList(),
            _ => new List<BattleCard>(),
        };

        if (!places.Contains(BattlePlaceId.None))
            effectCards.RemoveAll(x => !places.Contains(state.GetBelongUnit(x).GetBelongPlace(x).PlaceId));

        return mode[0] switch {
            "all"       => effectCards,
            "random"    => effectCards.Random(num, false),
            "first"     => effectCards.Take(num).ToList(),
            _           => effect.invokeTarget,
        };
    }

    public List<BattleCard> GetTarget(Effect effect, BattleState state) {
        var invokeUnit = effect.invokeUnit;
        var rhsUnit = state.GetRhsUnitById(invokeUnit.id);

        var source = effect.source;
        var sourceEffect = effect.sourceEffect;

        var allUnit = unit switch {
            "all"   =>  new List<BattleUnit>() { invokeUnit, rhsUnit },
            "me"    =>  new List<BattleUnit>() { invokeUnit },
            "op"    =>  new List<BattleUnit>() { rhsUnit },
            _       =>  new List<BattleUnit>(),
        };

        var allPlace = new List<BattlePlace>();
        var allCards = new List<BattleCard>();

        places.ForEach(placeId => { allUnit.ForEach(unit => allPlace.Add(unit.GetPlace(placeId))); });
        allPlace.ForEach(x => allCards.AddRange(x?.cards.Where(filter.FilterWithCurrentCard) ?? new List<BattleCard>()));

        allCards = ExcludeTargetByMode(allCards, effect);
        allCards = SortTargetByMode(allCards, effect);

        return mode[0] switch {
            "all"       => allCards,
            "random"    => allCards.Random(num, false),
            "first"     => allCards.Take(num).ToList(),
            "index"     => GetIndexTarget(allCards, invokeUnit),
            _ => effect.invokeTarget,
        };
    }
}