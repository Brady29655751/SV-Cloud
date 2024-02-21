using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAnimManager : Manager<BattleAnimManager>
{
    public bool IsSelectingTarget => targetView?.IsSelectingTarget ?? false;

    [SerializeField] private BattleStartView startView;
    [SerializeField] private BattleKeepCardView keepView;
    [SerializeField] private BattleTurnView turnView;

    [SerializeField] private BattleTargetView targetView;
    [SerializeField] private BattleUseAnimView useView;
    [SerializeField] private BattleEvolveAnimView evolveView;
    [SerializeField] private BattleAttackAnimView attackView;

    [SerializeField] private BattleDrawAnimView drawView;
    [SerializeField] private BattleDamageAnimView damageView;

    public override void Init()
    {
        base.Init();
        startView.ShowOrderInfo(keepView.ShowKeepInfo);
    }

    public void ResultAnim(string whosTurn, string description, Action callback) {
        turnView?.ShowBattleResult(whosTurn, description, callback);
    }

    public void KeepCardAnim(List<BattleCard> handCards, bool isOpDone, Action callback) {
        keepView?.ShowKeepResult(handCards, () => {
            if (isOpDone)
                keepView?.SetActive(false);

            callback?.Invoke();
        });
    }

    public void TurnStartAnim(string whosTurn, string description, Action callback) {
        turnView?.ShowTurnInfo(whosTurn, description, callback);
    }

    public void TargetAnim(string timing, BattleCard card, Action<List<short>> onSuccess, Action onFail) {
        targetView?.StartSelectTarget(timing, card, onSuccess, onFail);
    }

    public void UseAnim(int unitId, Card card, Action callback) {
        if (unitId == 0)
            useView?.MeUseCard(card, callback);
        else
            useView?.OpUseCard(card, callback);
    }

    public void EvolveAnim(BattleCardPlaceInfo info, BattleCard evolveCard, List<BattleCard> fieldCards, Action callback) {
        evolveView?.EvolveWithEP(info, evolveCard, fieldCards, callback);
    }
    
    public void AttackBeginDragAnim(int index) {
        attackView?.OnBeginDrag(index);
    }

    public void AttackDragAnim(int index) {
        attackView?.OnDrag(index);
    }

    public void AttackEndDragAnim(int index) {
        attackView?.OnEndDrag(index);
    }

    public void AttackAnim(int unitId, int index) {
        if (index < 0)
            return;
            
        attackView?.Attack(unitId, index);
    }
    
    public void DrawAnim(int unitId, bool currentHandMode, List<Card> inHand, List<Card> inGrave, Action callback) {
        if (unitId == 0)
            drawView?.MyDrawFromDeck(currentHandMode, inHand, inGrave, callback);
        else
            drawView?.OpDrawFromDeck(callback);
    }

    public void DamageAnim(int unitId, int index, int damage, Action callback) {
        var info = new BattleCardPlaceInfo() {
            unitId = unitId,
            place = (index == -1) ? BattlePlaceId.Leader : BattlePlaceId.Field,
            index = index,
        };
        damageView?.ShowDamage(info, damage, callback);
    }

    public void HealAnim(int unitId, int index, int heal, Action callback) {
        var info = new BattleCardPlaceInfo() {
            unitId = unitId,
            place = (index == -1) ? BattlePlaceId.Leader : BattlePlaceId.Field,
            index = index,
        };
        damageView?.ShowHeal(info, heal, callback);
    }

    public void LeaveFieldAnim(int unitId, int index, string type, Action callback) {
        var info = new BattleCardPlaceInfo() {
            unitId = unitId,
            place = (index == -1) ? BattlePlaceId.Leader : BattlePlaceId.Field,
            index = index,
        };
        damageView?.ShowLeaveField(info, type, callback);
    }

    public void GetTokenAnim(int unitId, bool hide, List<Card> tokens, Action callback) {
        if (unitId == 0)
            drawView?.MyGetToken(hide, tokens, callback);
        else
            drawView?.OpGetToken(hide, tokens, callback);
    }

    public void AddDeckAnim(int unitId, bool hide, List<Card> tokens, Action callback) {
        if (unitId == 0)
            drawView?.MyAddDeck(hide, tokens, callback);
        else
            drawView?.OpAddDeck(hide, tokens, callback);
    }

    public void BuryAnim(Action callback) {
        StartCoroutine(WaitForSeconds(0.75f, callback));
    }

}
