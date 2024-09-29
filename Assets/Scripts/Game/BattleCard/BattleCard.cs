using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleCard : IIdentifyHandler
{
    public Battle Battle => Player.currentBattle;
    public BattleManager Hud => BattleManager.instance;
    public static string[] FlagProperties => new string[] { "bane", "drain", "earth", "flag", "lastword" };

    public int Id => CurrentCard.Id;
    public bool IsEvolved { get; protected set; }
    public Card baseCard;
    public Card evolveCard;
    public Card OriginalCard => IsEvolved ? evolveCard : baseCard;
    public Card CurrentCard => GetCurrentCard(OriginalCard);
    public List<KeyValuePair<Func<bool>, Effect>> newEffects = new List<KeyValuePair<Func<bool>, Effect>>();
    public BattleCardBuffController buffController;
    public BattleCardActionController actionController;

    public Dictionary<string, int> options = new Dictionary<string, int>();
    
    public BattleCard(Card card) {
        IsEvolved = false;

        baseCard = (card == null) ? null : new Card(card);
        evolveCard = (baseCard?.EvolveCard == null) ? null : new Card(baseCard.EvolveCard);
        
        baseCard?.effects.ForEach(x => x.source = this);
        evolveCard?.effects.ForEach(x => x.source = this);

        buffController = new BattleCardBuffController();
        actionController = new BattleCardActionController(Mathf.RoundToInt(baseCard.GetIdentifier("maxAttackChance")));
    }

    public BattleCard(BattleCard rhs) {
        IsEvolved = rhs.IsEvolved;
        
        baseCard = (rhs.baseCard == null) ? null : new Card(rhs.baseCard);
        evolveCard = (rhs.evolveCard == null) ? null : new Card(rhs.evolveCard);

        baseCard?.effects.ForEach(x => x.source = this);
        evolveCard?.effects.ForEach(x => x.source = this);

        newEffects = rhs.newEffects.Select(x => new KeyValuePair<Func<bool>, Effect>
            (x.Key, new Effect(x.Value))).ToList();

        newEffects.ForEach(x => x.Value.source = this);

        buffController = new BattleCardBuffController(rhs.buffController);
        actionController = new BattleCardActionController(rhs.actionController);

        options = new Dictionary<string, int>(rhs.options);
    }

    public static BattleCard Get(Card baseCard) {
        return (baseCard == null) ? null : new BattleCard(baseCard);
    }

    public static BattleCard Get(int id) {
        return BattleCard.Get(Card.Get(id));
    }

    public bool TryGetIdenfier(string id, out int value)
    {
        value = GetIdentifier(id);
        return value != int.MinValue;
    }

    public int GetIdentifier(string id)
    {
        string trimId;

        if (id.TryTrimStart("current", out trimId)) {
            if (trimId.TryTrimParentheses(out var option)) {
                return option switch {
                    "IF" => BattleCardFilter.Parse(trimId.TrimStart("[IF]")).FilterWithCurrentCard(this) ? 1 : 0,
                    _ => 0
                };
            }
            return CurrentCard.GetIdentifier(trimId.TrimStart('.'));
        }

        if (id.TryTrimStart("base.", out trimId))
            return baseCard.GetIdentifier(trimId);

        if (id.TryTrimStart("evolve.", out trimId))
            return evolveCard.GetIdentifier(trimId);

        if (id.TryTrimStart("buff.", out trimId))
            return buffController.GetIdentifier(trimId);

        if (id.TryTrimStart("action.", out trimId))
            return actionController.GetIdentifier(trimId);

        return id switch {
            _ => options.Get(id, 0),
        };
    }

    public void SetIdentifier(string id, int value)
    {
        var trimId = string.Empty;
        
        if (id.TryTrimStart("base.", out trimId))
            baseCard.SetIdentifier(trimId, value);

        else if (id.TryTrimStart("evolve.", out trimId))
            evolveCard.SetIdentifier(trimId, value);
        
        else if (id.TryTrimStart("buff.", out trimId))
            buffController.SetIdentifier(trimId, value);

        else if (id.TryTrimStart("action.", out trimId))
            actionController.SetIdentifier(trimId, value);

        else 
            options.Set(id, value);
    }

    public Card GetCurrentCard(Card baseCard) {
        if (baseCard == null)
            return null;
            
        var result = new Card(baseCard);
        result.cost = Mathf.Max(result.cost + buffController.CostBuff, 0);
        result.atk = Mathf.Max(result.atk + buffController.AtkBuff, 0);
        result.hpMax = Mathf.Max(result.hpMax + buffController.HpBuff, 0);
        result.hp = Mathf.Max(result.hpMax - buffController.Damage, 0);
        result.effects.AddRange(newEffects.Select(x => x.Value));
        result.effects.ForEach(x => x.source = this);
        return result;
    }

    public BattleCard GetCurrentBattleCard(int cost, string situation) {
        if (string.IsNullOrEmpty(situation))
            return this;

        var ability = situation.ToEffectAbility();
        var effect = CurrentCard.effects.Find(x => (x.ability == ability) && (int.Parse(x.abilityOptionDict.Get("accelerate", "-1")) == cost));
        if (effect == null)
            return this;

        return BattleCard.Get(int.Parse(effect.abilityOptionDict.Get("id", "-1"))) ?? this;
    }

    #region card-description

    public string GetConditionDescription() {
        var card = CurrentCard;
        var description = string.Empty;
        var leaderInfoKeys = new List<string>();
        var sourceInfoKeys = new List<string>();

        for (int i = 0; i < card.effects.Count; i++) {
            var currentEffect = card.effects[i];
            if (List.IsNullOrEmpty(currentEffect.condition))
                continue;

            for (int j = 0; j < currentEffect.condition.Count; j++) {
                var currentCondition = currentEffect.condition[j];
                if (currentCondition.TryTrimStart("leader.", out var leaderKey))
                    leaderInfoKeys.Add(leaderKey);
                else if (currentCondition.TryTrimStart("source.", out var sourceKey))
                    sourceInfoKeys.Add(sourceKey);
            }
        }
        leaderInfoKeys = leaderInfoKeys.Distinct().ToList();
        sourceInfoKeys = sourceInfoKeys.Distinct().ToList();

        for (int i = 0; i < leaderInfoKeys.Count; i++) {
            var num = Hud.CurrentState.GetBelongUnit(this).leader.GetIdentifier(leaderInfoKeys[i]);
            description += "（當前 " + leaderInfoKeys[i].ToLeaderInfoValue() + " 為 " + num + "）\n";
        }

        for (int i = 0; i < sourceInfoKeys.Count; i++) {
            description += "（當前 " + sourceInfoKeys[i].ToSourceInfoValue() + " 為 " + GetIdentifier(sourceInfoKeys[i]) + "）\n";
        }

        return description;
    }

    public string GetAdditionalDescription() {
        var description = string.Empty;
        var costBuff = buffController.CostBuff;
        var atkBuff = buffController.AtkBuff;
        var hpBuff = buffController.HpBuff;
        var originalKeywords = Card.Get(Id).keywords;

        description += atkBuff.ToStringWithSign() + "/" + hpBuff.ToStringWithSign() + "\n";

        // if (costBuff != 0)
        //     description += "消費 " + costBuff.ToStringWithSign() + "\n";

        var keywords = CardDatabase.KeywordEffects.Where(x => !originalKeywords.Contains(x)).ToList();
        var keywordCount = 0;
        for (int i = 0; i < keywords.Count; i++) {
            if (actionController.IsKeywordAvailable(keywords[i])) {
                description += ("[ffbb00]" + keywords[i].GetKeywordName() + "[-]").GetDescription() + "\n";
                keywordCount++;
            }
        }

        var evolveCost = GetEvolveCost();
        if (evolveCost != BattleCard.Get(CurrentCard.id).GetEvolveCost()) {
            description += ((evolveCost == 0) ? "不消費EP即可進化" : ("消費 " + evolveCost + " EP才可進化" )) + "。\n";
            keywordCount++;
        }

        var newEffectDescription = string.Empty;
        for (int i = 0; i < newEffects.Count; i++) {
            if (newEffects[i].Value.hudOptionDict.TryGetValue("description", out var effectDesc) && (!string.IsNullOrEmpty(effectDesc)))
                newEffectDescription += effectDesc + "\n";
        }

        if (!string.IsNullOrEmpty(newEffectDescription))
            description += ((keywordCount > 0) ? "------\n" : string.Empty) + newEffectDescription;

        return (description.Count(x => x == '\n') == 1) ? string.Empty : description.TrimEnd('\n');
    }

    #endregion

    #region target-effects

    public void GetTargetEffectWithTiming(string timing, out Queue<Effect> targetEffectQueue, out Queue<EffectTargetInfo> targetInfoQueue, out Queue<List<int>> selectableTargetQueue) {
        bool isEvolveTiming = timing == "on_this_evolve_with_ep";

        var nowCost = GetUseCost(Hud.CurrentState.myUnit.leader, out var situation);
        var nowBattleCard = GetCurrentBattleCard(nowCost, situation);
        var nowCard = GetCurrentCard(isEvolveTiming ? nowBattleCard.evolveCard : nowBattleCard.OriginalCard);

        targetEffectQueue = new Queue<Effect>();
        targetInfoQueue = new Queue<EffectTargetInfo>();
        selectableTargetQueue = new Queue<List<int>>();

        for (int i = 0; i < nowCard.effects.Count; i++) {
            var currentEffect = nowCard.effects[i];
            if (currentEffect.timing != timing)
                continue;

            currentEffect.invokeUnit = Hud.CurrentState.myUnit;
            if (currentEffect.Condition(Hud.CurrentState)) {
                var info = currentEffect.GetEffectTargetInfo(Hud.CurrentState);

                if ((!List.IsNullOrEmpty(info.mode)) && (info.mode[0] == "index")) {
                    targetEffectQueue.Enqueue(currentEffect);
                    targetInfoQueue.Enqueue(info);
                    selectableTargetQueue.Enqueue(GetCurrentSelectableTarget(info));
                }

                var appendixEffect = currentEffect;
                while (appendixEffect.abilityOptionDict.TryGetValue("appendix", out var appendixId)) {
                    appendixEffect = Effect.Get(int.Parse(appendixId));
                    if (appendixEffect == null)
                        break;

                    appendixEffect.invokeUnit = Hud.CurrentState.myUnit;
                    if (appendixEffect.Condition(Hud.CurrentState)) {
                        var appendixInfo = appendixEffect.GetEffectTargetInfo(Hud.CurrentState);

                        if ((!List.IsNullOrEmpty(appendixInfo.mode)) && (appendixInfo.mode[0] == "index")) {
                            targetEffectQueue.Enqueue(appendixEffect);
                            targetInfoQueue.Enqueue(appendixInfo);
                            selectableTargetQueue.Enqueue(GetCurrentSelectableTarget(appendixInfo));
                        }
                    }
                    appendixEffect.invokeUnit = null;
                }
            }
            currentEffect.invokeTarget = null;
        }
    }

    public List<int> GetCurrentSelectableTarget(EffectTargetInfo currentInfo) {
        List<int> currentSelectableList = new List<int>();

        if (currentInfo.places.Contains(BattlePlaceId.Hand)) {
            var myHand = Hud.CurrentState.myUnit.hand;
            var opHand = Hud.CurrentState.opUnit.hand;

            var myIndex = (currentInfo.unit == "op") ? new List<int>() : 
                Enumerable.Range(0, myHand.Count).Where(x => currentInfo.filter.FilterWithCurrentCard(myHand.cards[x])).ToList();
                    
            var opIndex = (currentInfo.unit == "me") ? new List<int>() : 
                Enumerable.Range(0, opHand.Count).Where(x => opHand.cards[x].IsTargetSelectable() && 
                    currentInfo.filter.FilterWithCurrentCard(opHand.cards[x])).ToList();
            
            myIndex.ForEach(x => currentSelectableList.Add((int)BattlePlaceId.Hand * 10 + x));
            opIndex.ForEach(x => currentSelectableList.Add(100 + (int)BattlePlaceId.Hand * 10 + x));
        }

        if (currentInfo.places.Contains(BattlePlaceId.Field)) {
            var myField = Hud.CurrentState.myUnit.field;
            var opField = Hud.CurrentState.opUnit.field;

            var myIndex = (currentInfo.unit == "op") ? new List<int>() : 
                Enumerable.Range(0, myField.Count).Where(x => currentInfo.filter.FilterWithCurrentCard(myField.cards[x])).ToList();
                    
            var opIndex = (currentInfo.unit == "me") ? new List<int>() : 
                Enumerable.Range(0, opField.Count).Where(x => opField.cards[x].IsTargetSelectable() && 
                    currentInfo.filter.FilterWithCurrentCard(opField.cards[x])).ToList();

            myIndex.ForEach(x => currentSelectableList.Add((int)BattlePlaceId.Field * 10 + x));
            opIndex.ForEach(x => currentSelectableList.Add(100 + (int)BattlePlaceId.Field * 10 + x));
        }

        if (currentInfo.places.Contains(BattlePlaceId.Leader)) {
            if (currentInfo.unit != "op")
                currentSelectableList.Add((int)BattlePlaceId.Leader * 10);

            if (currentInfo.unit != "me")
                currentSelectableList.Add(100 + (short)BattlePlaceId.Leader * 10);
        }

        if (currentInfo.places.Contains(BattlePlaceId.Token))
            currentSelectableList.AddRange(currentInfo.effect.abilityOptionDict.Get("token", string.Empty).ToIntList('/'));

        if (currentInfo.mode.Contains("other"))
            currentSelectableList.Remove(Hud.CurrentState.GetCardPlaceInfo(this).ToIntCode());

        return currentSelectableList;
    }

    #endregion

    #region  is-condition

    public int GetUseCost(Leader leader, out string situation) {
        var card = CurrentCard;
        var effects = card.effects;

        List<int> GetSecondChoiceCostList(string choiceSituation, out int choiceCost) {
            var choiceList = new List<int>() { -1 };
            for (int i = 0; i < effects.Count; i++) {
                var currentEffect = effects[i];
                if (currentEffect.abilityOptionDict.TryGetValue(choiceSituation, out var choiceCostId))
                    choiceList.Add(int.Parse(choiceCostId));
            }

            choiceCost = choiceList.Where(x => x <= leader.PP).Max();
            return choiceList;
        }

        var enhanceList = GetSecondChoiceCostList("enhance", out var enhanceCost);
        if (enhanceCost != -1) {
            situation = "enhance";
            return enhanceCost;
        }

        var accelList = GetSecondChoiceCostList("accelerate", out var accelCost);
        var crystalList = GetSecondChoiceCostList("crystalize", out var crystalCost);

        // If cost enough or no accel/crystal effect
        if ((card.cost <= leader.PP) || ((accelList.Count <= 1) && (crystalList.Count <= 1))) {
            situation = string.Empty;
            return card.cost;
        }

        // If accel/crystal effect exists but not enough cost => show min cost.
        if ((accelCost == -1) && (crystalCost == -1)) {
            accelCost = accelList.Where(x => x >= 0).DefaultIfEmpty(-1).Min();
            crystalCost = crystalList.Where(x => x >= 0).DefaultIfEmpty(-1).Min();
        }

        bool isAccel = accelCost >= crystalCost;
        situation = isAccel ? "accelerate" : "crystalize";
        return isAccel ? accelCost : crystalCost;
    }

    public bool IsUsable(BattleUnit sourceUnit) {
        var useCost = GetUseCost(sourceUnit.leader, out var situation);
        var useCard = GetCurrentBattleCard(useCost, situation);

        bool isCostEnough = sourceUnit.leader.PP >= useCost;
        bool isFieldFull = useCard.CurrentCard.IsFollower() && sourceUnit.field.IsFull;
        bool isSpellTargetable = true;

        if (useCard.CurrentCard.Type == CardType.Spell) {
            GetTargetEffectWithTiming("on_this_use", out _, out var infoQueue, out var selectableQueue);

            while (infoQueue.Count > 0) {
                isSpellTargetable = infoQueue.Dequeue().num <= selectableQueue.Dequeue().Count;
                if (!isSpellTargetable)
                    break;
            }
        }

        return sourceUnit.isMyTurn && isCostEnough && (!isFieldFull) && isSpellTargetable;
    }

    public int GetEvolveCost() {
        return Mathf.CeilToInt(CurrentCard.GetIdentifier("evolveCost"));
    }

    public bool IsEvolvable(BattleUnit sourceUnit) {
        bool isUnevolvedFollower = CurrentCard.Type == CardType.Follower;
        bool isEpUnused = (sourceUnit.isEvolveEnabled) && (!sourceUnit.leader.isEpUsed) && (sourceUnit.leader.EP >= GetEvolveCost());
        return isUnevolvedFollower && isEpUnused;
    }

    public bool IsAttackable(BattleUnit sourceUnit) {
        return IsLeaderAttackable(sourceUnit) || IsFollowerAttackable(sourceUnit);
    }

    public bool IsLeaderAttackable(BattleUnit sourceUnit) {
        var isAttackChanceLegal = (actionController.CurrentAttackChance > 0) && (!actionController.IsKeywordAvailable(CardKeyword.Freeze));
        var isStayTurnLegal = actionController.StayFieldTurn > 0;
        var isKeywordLegal = actionController.IsKeywordAvailable(CardKeyword.Storm);

        return sourceUnit.isMyTurn && isAttackChanceLegal && (isStayTurnLegal || isKeywordLegal);
    }

    public bool IsFollowerAttackable(BattleUnit sourceUnit) {
        var isAttackChanceLegal = (actionController.CurrentAttackChance > 0) && (!actionController.IsKeywordAvailable(CardKeyword.Freeze));
        var isStayTurnLegal = actionController.StayFieldTurn > 0;
        var isKeywordLegal = actionController.IsKeywordAvailable(CardKeyword.Storm) || actionController.IsKeywordAvailable(CardKeyword.Rush);

        return sourceUnit.isMyTurn && isAttackChanceLegal && (IsEvolved || isStayTurnLegal || isKeywordLegal);
    }

    public bool IsTargetSelectable() {
        bool isAmbush = actionController.IsKeywordAvailable(CardKeyword.Ambush);
        bool isAura = actionController.IsKeywordAvailable(CardKeyword.Aura);
        return (!isAmbush) && (!isAura);
    }

    #endregion

    // Evolve this follower. You should check IsEvolvable() before calling this if you use EP evolve.
    public void Evolve() {
        IsEvolved = true;
    }

    public void SetKeyword(Func<bool> untilFunc, CardKeyword keyword, ModifyOption option) {
        if (option == ModifyOption.Add) {
            baseCard.keywords.Add(keyword);
            evolveCard?.keywords.Add(keyword);
            actionController.SetKeyword(untilFunc, keyword);
        } else if (option == ModifyOption.Remove) {
            baseCard.keywords.RemoveAll(x => x == keyword);
            evolveCard?.keywords.RemoveAll(x => x == keyword);
            actionController.RemoveKeyword(keyword);
        }
    }

    #region take-effects
    private List<Effect> GetValidTakeEffects(List<Effect> originalEffects, Effect effect, BattleState state, string originalId, int replaceId) {
        List<Effect> validEffects = new List<Effect>();

        if (originalEffects.Count <= 0)
            return validEffects;
        
        for (int i = 0; i < originalEffects.Count; i++) {
            originalEffects[i].source = effect.source;
            originalEffects[i].invokeUnit = state.GetBelongUnit(this);
            originalEffects[i].condOptionDictList.ForEach(x => x.ForEach(y => {
                if (y.lhs == originalId)
                    y.lhs = "[num]" + replaceId.ToString();
            }));

            if (originalEffects[i].Condition(state))
                validEffects.Add(originalEffects[i]);

            originalEffects[i].source = null;
            originalEffects[i].invokeUnit = null;
            originalEffects[i].condOptionDictList.ForEach(x => x.ForEach(y => {
                if (y.lhs.StartsWith("[num]"))
                    y.lhs = originalId;
            }));
        }

        return validEffects;
    }

    public int TakeDamage(int damage, Effect effect, BattleState state) {
        var damageEffects = CurrentCard.effects.Where(x => x.ability == EffectAbility.SetDamage);
        var addEffects = damageEffects.Where(x => x.abilityOptionDict.ContainsKey("add")).ToList();
        var setEffects = damageEffects.Where(x => x.abilityOptionDict.ContainsKey("set")).ToList();

        addEffects = GetValidTakeEffects(addEffects, effect, state, "damage", damage);
        addEffects.ForEach(x => damage += Parser.ParseEffectExpression(x.abilityOptionDict.Get("add"), effect, state));

        setEffects = GetValidTakeEffects(setEffects, effect, state, "damage", damage);
        if (setEffects.Count > 0)
            damage = setEffects.Select(x => Parser.ParseEffectExpression(x.abilityOptionDict.Get("set"), effect, state)).Min();

        return buffController.TakeDamage(damage);
    }

    public int TakeHeal(int heal, Effect effect, BattleState state) {
        var healEffects = CurrentCard.effects.Where(x => x.ability == EffectAbility.SetHeal);
        var addEffects = healEffects.Where(x => x.abilityOptionDict.ContainsKey("add")).ToList();
        var setEffects = healEffects.Where(x => x.abilityOptionDict.ContainsKey("set")).ToList();
        
        addEffects = GetValidTakeEffects(addEffects, effect, state, "heal", heal);
        addEffects.ForEach(x => heal += Parser.ParseEffectExpression(x.abilityOptionDict.Get("add"), effect, state));

        setEffects = GetValidTakeEffects(setEffects, effect, state, "heal", heal);
        if (setEffects.Count > 0)
            heal = setEffects.Select(x => Parser.ParseEffectExpression(x.abilityOptionDict.Get("set"), effect, state)).Min();

        return buffController.TakeHeal(heal);
    }

    public void TakeBuff(CardStatus status, Func<bool> untilCondition) {
        buffController.TakeBuff(status, untilCondition);
    }

    public int TakeCountdown(int add) {
        if (baseCard.countdown > 0)
            baseCard.countdown = Mathf.Max(baseCard.countdown + add, 0); 
            
        return add;
    }

    #endregion

    #region remove-effects

    public void RemoveUntilEffect() {
        newEffects.RemoveAll(x => (x.Key != null) && (x.Key.Invoke()));
        buffController.RemoveUntilEffect();    
        actionController.RemoveUntilEffect();
    }

    public void RemoveEffect(Effect effect) {
        if (effect.id == 0) {
            newEffects.RemoveAll(x => x.Value == effect);
            return;
        }

        baseCard.ClearEffect(effect.id);
        evolveCard.ClearEffect(effect.id);
    }

    public void RemoveEffectWithTiming(string timing = "all") {
        baseCard.ClearEffects(timing);
        evolveCard.ClearEffects(timing);
        newEffects.RemoveAll(x => (timing == "all") || (timing == x.Value.timing));

        if (timing == "all") {    
            foreach (var keyword in CardDatabase.KeywordEffects)
                SetKeyword(null, keyword, ModifyOption.Remove);
        }
    }

    #endregion
}
