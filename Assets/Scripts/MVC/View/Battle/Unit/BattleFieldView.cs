using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleFieldView : BattleBaseView
{
    [SerializeField] private int id;
    [SerializeField] private List<BattleCardView> cardViews;

    public List<BattleCard> fieldCards = new List<BattleCard>();
    public int fieldCount => fieldCards.Count;

    public void SetField(BattleField field) {
        fieldCards = field.cards.ToList();
        for (int i = 0; i < cardViews.Count; i++) {
            cardViews[i].SetBattleCard((i < field.Count) ? field.cards[i] : null);
        }
    } 

    public void ShowFieldInfo(int index) {
        var unit = Hud.CurrentState.myUnit;
        var card = (index < fieldCount) ? fieldCards[index] : null;

        if (Anim.IsSelectingTarget)
            return;

        Hud.CurrentCardPlaceInfo = new BattleCardPlaceInfo() { 
            unitId = id,
            place = BattlePlaceId.Field,
            index = index,
        };

        cardInfoView?.SetBattleCard(card, () => {
            if (id != 0)
                return;

            cardInfoView?.buttonView?.SetEvolvable((card?.IsEvolvable(unit) ?? false) && unit.isMyTurn);
        });
    }

    public void OnBeginDrag(int index) {
        if (!index.IsInRange(0, fieldCount))
            return;

        var unit = (id == 0) ? Hud.CurrentState.myUnit : Hud.CurrentState.opUnit;

        if (!fieldCards[index].IsAttackable(unit))
            return;

        Hud.CurrentCardPlaceInfo = new BattleCardPlaceInfo() { 
            unitId = id,
            place = BattlePlaceId.Field,
            index = index,
        };

        cardInfoView?.SetBattleCard(null);

        Anim.AttackBeginDragAnim(index);
    }

    public void OnDrag(int index) {
        if (!index.IsInRange(0, fieldCount))
            return;

        var unit = (id == 0) ? Hud.CurrentState.myUnit : Hud.CurrentState.opUnit;

        if (!fieldCards[index].IsAttackable(unit))
            return;

        Anim.AttackDragAnim(index);
    }

    public void OnEndDrag(int index) {
        if (!index.IsInRange(0, fieldCount))
            return;

        var unit = (id == 0) ? Hud.CurrentState.myUnit : Hud.CurrentState.opUnit;

        if (!fieldCards[index].IsAttackable(unit))
            return;
            
        Anim.AttackEndDragAnim(index);
    }

    public void Evolve() {
        var info = Hud.CurrentCardPlaceInfo;
        if (Hud.IsLocked || (info.unitId != 0) || (info.place != BattlePlaceId.Field))
            return;

        if (!Battle.CurrentState.myUnit.isMyTurn)
            return;
        
        var card = info.GetBattleCard(Battle.CurrentState);
        Anim.TargetAnim("on_this_evolve_with_ep", card, (target) => OnEvolveSuccess(info.index, target), null);
    }

    private void OnEvolveSuccess(int index, List<int> target) {
        int[] data = (new int[] { (int)EffectAbility.Evolve, index }).Concat(target).ToArray();
        Battle.PlayerAction(data, true);
    }

}
