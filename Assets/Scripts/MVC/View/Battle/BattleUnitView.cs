using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        SetUnit((id == 0) ? Battle.currentState.myUnit : Battle.currentState.opUnit);
    }

    public void SetState(BattleState state) {
        var effect = state.currentEffect;
        var unit = (id == 0) ? state.myUnit : state.opUnit;
        var invokeUnit = effect.invokeUnit;

        IsDone = false;

        if (invokeUnit.id == unit.id)
            SetMyState(state, invokeUnit);
        else 
            SetOpState(state, unit);
    }

    private void SetMyState(BattleState state, BattleUnit unit) {
        var effect = state.currentEffect;

        switch (effect.ability) {
            default:
                SetUnit(unit);
                break;
            case EffectAbility.Use:
                StartCoroutine(WaitForSeconds(0.5f, () => {
                    handView?.SetHandMode(true);
                    SetUnit(unit);
                }));
                break;
        };
    }

    private void SetOpState(BattleState state, BattleUnit unit) {
        var effect = state.currentEffect;

        switch (effect.ability) {
            default:
                SetUnit(unit);
                break;
            case EffectAbility.Use:
                SetUnit(unit);
                break;
        };
    }

    private void SetUnit(BattleUnit unit) {
        leaderView?.SetLeader(unit?.leader);
        ppView.SetLeader(unit?.leader);
        ppView?.SetTurnEndButtonActive((unit == null) ? false : unit.isMyTurn && (!unit.isDone));
        epView?.SetLeader(unit?.leader);
        handView?.SetHand(unit);
        fieldView?.SetField(unit?.field);
        deckView?.SetDeck(unit?.deck);
        cornerView.SetUnit(unit);

        IsDone = true;
    }
}
