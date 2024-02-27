using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class BattleUnitView : BattleBaseView
{
    [SerializeField] private int id;
    [SerializeField] private BattleLeaderView leaderView;
    [SerializeField] private BattlePPView ppView;
    [SerializeField] private BattleEPView epView;
    [SerializeField] private BattleHandView handView;
    [SerializeField] private BattleFieldView fieldView;
    [SerializeField] private BattleDeckView deckView;
    [SerializeField] private BattleCornerView cornerView;

    public bool IsDone { get; protected set; } = true;

    public override void Init()
    {
        base.Init();
        SetUnit((id == 0) ? Battle.CurrentState.myUnit : Battle.CurrentState.opUnit);
    }

    [PunRPC]
    private void RPCPlayerAction(short[] data) {
        Battle.PlayerAction(data.Select(x => (int)x).ToArray(), false);
    }

    public void SetLock(bool isLocked) {
        if (id != 0)
            return;

        handView.SetLock(isLocked);
    }

    public void SetState(BattleState state) {
        IsDone = false;

        if (id == 0) 
            SetMyState(state);
        else
            SetOpState(state);
    }

    private void SetMyState(BattleState state) {
        var effect = state.currentEffect;
        var invokeUnit = effect.invokeUnit;
        var unit = state.myUnit;
        var who = "me";

        Action SetMyUnit = (() => SetUnit(unit));

        switch (effect.ability) {
            default:
                SetMyUnit();
                break;

            case EffectAbility.Use:
                if (invokeUnit.id != unit.id)
                    goto default;

                var status = effect.hudOptionDict.Get("status", "none").ToIntList('/');
                var card = new Card(effect.invokeTarget[0].CurrentCard)
                {
                    cost = status[0],
                    atk = status[1],
                    hp = status[2],
                };

                SetUnit(unit, false);
                Anim.UseAnim(0, card, SetMyUnit);
                break;

            case EffectAbility.Attack:
                if (invokeUnit.id != unit.id)
                    goto default;

                var sourceIndex = int.Parse(effect.hudOptionDict.Get("source", "-1"));
                Anim.AttackAnim(0, sourceIndex);
                SetMyUnit();
                break;

            case EffectAbility.Evolve:
                if (invokeUnit.id != unit.id)
                    goto default;

                var evolveIndex = int.Parse(effect.hudOptionDict.Get("index", "-1"));
                if (evolveIndex >= 0) {
                    // Evolve with EP.
                    Anim.EvolveAnim(new BattleCardPlaceInfo() {
                        unitId = 0,
                        place = BattlePlaceId.Field,
                        index = evolveIndex,
                    }, unit.field.cards[evolveIndex], fieldView.fieldCards, SetMyUnit);
                } else {
                    // Auto evolve.
                    SetMyUnit();
                }

                break;

            case EffectAbility.Draw:
                var whoDraw = effect.hudOptionDict.Get("who", "me");
                if (whoDraw != who)
                    goto default;

                var drawCount = int.Parse(effect.hudOptionDict.Get("count"));
                var inHand = effect.invokeTarget.Select(x => x.CurrentCard).ToList();
                var inGrave = unit.grave.cards.TakeLast(drawCount - effect.invokeTarget.Count).Select(x => x.CurrentCard).ToList();

                Anim.DrawAnim(0, handView.Mode, inHand, inGrave, SetMyUnit);
                break;

            case EffectAbility.Damage:
                var damageIndexList = effect.hudOptionDict.Get("myIndex", string.Empty).ToIntList('/');
                var damageValueList = effect.hudOptionDict.Get("myDamage", string.Empty).ToIntList('/');

                if (List.IsNullOrEmpty(damageIndexList) || List.IsNullOrEmpty(damageValueList))
                    goto default;

                for (int i = 0; i < damageIndexList.Count; i++) {
                    Anim.DamageAnim(0, damageIndexList[i], damageValueList[i],
                        (i == damageIndexList.Count - 1) ? SetMyUnit : null);
                }
                break;

            case EffectAbility.Heal:
                var healIndexList = effect.hudOptionDict.Get("myIndex", string.Empty).ToIntList('/');
                var healValueList = effect.hudOptionDict.Get("myHeal", string.Empty).ToIntList('/');

                if (List.IsNullOrEmpty(healIndexList) || List.IsNullOrEmpty(healValueList))
                    goto default;

                for (int i = 0; i < healIndexList.Count; i++) {
                    Anim.HealAnim(0, healIndexList[i], healValueList[i], 
                        (i == healIndexList.Count - 1) ? SetMyUnit : null);
                }
                break;

            case EffectAbility.Destroy:
            case EffectAbility.Vanish:
            case EffectAbility.Return:
                var leaveFieldIndexList = effect.hudOptionDict.Get("myIndex", string.Empty).ToIntList('/');
                var leaveFieldValueList = effect.hudOptionDict.Get("myValue", string.Empty).Split('/').ToList();
                var lastVanishIndex = leaveFieldValueList.LastIndexOf("vanish");
                var lastCallbackIndex = (lastVanishIndex == -1) ? (leaveFieldIndexList.Count - 1) : lastVanishIndex;

                if (List.IsNullOrEmpty(leaveFieldIndexList) || List.IsNullOrEmpty(leaveFieldValueList))
                    goto default;

                for (int i = 0; i < leaveFieldIndexList.Count; i++) {
                    Anim.LeaveFieldAnim(0, leaveFieldIndexList[i], leaveFieldValueList[i], 
                        (i == lastCallbackIndex) ? SetMyUnit : null);
                }
                break;

            case EffectAbility.GetToken:
                var whoGetToken = effect.hudOptionDict.Get("who", "me");
                if (whoGetToken != who)
                    goto default;

                var tokenHide = bool.Parse(effect.hudOptionDict.Get("hide", "false"));
                var tokenList = effect.hudOptionDict.Get("token", string.Empty).ToIntList('/');
                Anim.GetTokenAnim(0, tokenHide, tokenList.Select(Card.Get).ToList(), SetMyUnit);
                break;

            case EffectAbility.AddDeck:
                var whoAddDeck = effect.hudOptionDict.Get("who", "me");
                if (whoAddDeck != who)
                    goto default;

                var addDeckHide = bool.Parse(effect.hudOptionDict.Get("hide", "false"));
                var addDeckList = effect.hudOptionDict.Get("token", string.Empty).ToIntList('/');
                Anim.AddDeckAnim(0, addDeckHide, addDeckList.Select(Card.Get).ToList(), SetMyUnit);
                break;

            case EffectAbility.Bury:
                if (invokeUnit.id != unit.id)
                    goto default;

                Anim.BuryAnim(SetMyUnit);
                break;

            case EffectAbility.Reveal:
            if (invokeUnit.id != unit.id)
                goto default;

            Anim.RevealAnim(0, effect.invokeTarget.Select(x => x.CurrentCard).ToList(), SetMyUnit);
            break;
        };
    }

    private void SetOpState(BattleState state) {
        var effect = state.currentEffect;
        var invokeUnit = effect.invokeUnit;
        var unit = state.opUnit;
        var who = "op";

        Action SetOpUnit = (() => SetUnit(unit));

        switch (effect.ability) {
            default:
                SetOpUnit();
                break;

            case EffectAbility.Use:
                if (invokeUnit.id != unit.id)
                    goto default;

                var status = effect.hudOptionDict.Get("status", "none").ToIntList('/');
                var card = new Card(effect.invokeTarget[0].CurrentCard)
                {
                    cost = status[0],
                    atk = status[1],
                    hp = status[2],
                };

                SetUnit(unit, false);
                Anim.UseAnim(1, card, SetOpUnit);
                break;

            case EffectAbility.Attack:
                if (invokeUnit.id != unit.id)
                    goto default;

                var sourceIndex = int.Parse(effect.hudOptionDict.Get("source", "-1"));
                Anim.AttackAnim(1, sourceIndex);
                SetOpUnit();
                break;

            case EffectAbility.Evolve:
                if (invokeUnit.id != unit.id)
                    goto default;

                var index = int.Parse(effect.hudOptionDict.Get("index", "-1"));
                if (index >= 0) {
                    // Evolve with EP.
                    Anim.EvolveAnim(new BattleCardPlaceInfo() {
                        unitId = 1,
                        place = BattlePlaceId.Field,
                        index = index,
                    }, unit.field.cards[index], fieldView.fieldCards, SetOpUnit);
                } else {
                    // Auto evolve.
                    SetOpUnit();
                }

                break;

            case EffectAbility.Draw:
                var whoDraw = effect.hudOptionDict.Get("who", "op");
                if (whoDraw != who)
                    goto default;
                
                var count = int.Parse(effect.hudOptionDict.Get("count"));
                var inHand = effect.invokeTarget.Select(x => x.CurrentCard).ToList();
                var inGrave = unit.grave.cards.TakeLast(count - effect.invokeTarget.Count).Select(x => x.CurrentCard).ToList();

                if (count <= 0)
                    goto default;

                Anim.DrawAnim(1, false, inHand, inGrave, SetOpUnit);
                break;

            case EffectAbility.Damage:
                var damageIndexList = effect.hudOptionDict.Get("opIndex", string.Empty).ToIntList('/');
                var damageValueList = effect.hudOptionDict.Get("opDamage", string.Empty).ToIntList('/');

                if (List.IsNullOrEmpty(damageIndexList) || List.IsNullOrEmpty(damageValueList))
                    goto default;

                for (int i = 0; i < damageIndexList.Count; i++) {
                    Anim.DamageAnim(1, damageIndexList[i], damageValueList[i],
                        (i == damageIndexList.Count - 1) ? SetOpUnit : null);
                }   
                break;

            case EffectAbility.Heal:
                var healIndexList = effect.hudOptionDict.Get("opIndex", string.Empty).ToIntList('/');
                var healValueList = effect.hudOptionDict.Get("opHeal", string.Empty).ToIntList('/');
        
                if (List.IsNullOrEmpty(healIndexList) || List.IsNullOrEmpty(healValueList))
                    goto default;

                for (int i = 0; i < healIndexList.Count; i++) {
                    Anim.HealAnim(1, healIndexList[i], healValueList[i], 
                        (i == healIndexList.Count - 1) ? SetOpUnit : null);
                }
                break;

            case EffectAbility.Destroy:
            case EffectAbility.Vanish:
            case EffectAbility.Return:
                var leaveFieldIndexList = effect.hudOptionDict.Get("opIndex", string.Empty).ToIntList('/');
                var leaveFieldValueList = effect.hudOptionDict.Get("opValue", string.Empty).Split('/').ToList();
                var lastVanishIndex = leaveFieldValueList.LastIndexOf("vanish");
                var lastCallbackIndex = (lastVanishIndex == -1) ? (leaveFieldIndexList.Count - 1) : lastVanishIndex;

                if (List.IsNullOrEmpty(leaveFieldIndexList) || List.IsNullOrEmpty(leaveFieldValueList))
                    goto default;

                for (int i = 0; i < leaveFieldIndexList.Count; i++) {
                    
                    Anim.LeaveFieldAnim(1, leaveFieldIndexList[i], leaveFieldValueList[i], 
                        (i == lastCallbackIndex) ? SetOpUnit : null);
                }
                break;

            case EffectAbility.GetToken:
                var whoGetToken = effect.hudOptionDict.Get("who", "op");
                if (whoGetToken != who)
                    goto default;

                var tokenHide = bool.Parse(effect.hudOptionDict.Get("hide", "false"));
                var tokenList = effect.hudOptionDict.Get("token", string.Empty).ToIntList('/');
                Anim.GetTokenAnim(1, tokenHide, tokenList.Select(Card.Get).ToList(), SetOpUnit);
                break;

            case EffectAbility.AddDeck:
                var whoAddDeck = effect.hudOptionDict.Get("who", "op");
                if (whoAddDeck != who)
                    goto default;

                var addDeckHide = bool.Parse(effect.hudOptionDict.Get("hide", "false"));
                var addDeckList = effect.hudOptionDict.Get("token", string.Empty).ToIntList('/');
                Anim.AddDeckAnim(1, addDeckHide, addDeckList.Select(Card.Get).ToList(), SetOpUnit);
                break;

            case EffectAbility.Bury:
                if (invokeUnit.id != unit.id)
                    goto default;

                Anim.BuryAnim(SetOpUnit);
                break;
            
            case EffectAbility.Reveal:
                if (invokeUnit.id != unit.id)
                    goto default;

                Anim.RevealAnim(1, effect.invokeTarget.Select(x => x.CurrentCard).ToList(), SetOpUnit);
                break;
        };
    }

    private void SetUnit(BattleUnit unit, bool setDone = true) {
        leaderView?.SetLeader(unit?.leader);
        ppView.SetLeader(unit?.leader);
        ppView?.SetTurnEndButtonActive((unit == null) ? false : unit.isMyTurn && (!unit.isDone));
        epView?.SetLeader(unit?.leader);
        handView?.SetHand(unit);
        fieldView?.SetField(unit?.field);
        deckView?.SetDeck(unit?.deck);
        cornerView.SetUnit(unit);

        IsDone = setDone;
    }
}
