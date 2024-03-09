using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleBaseView : IMonoBehaviour
{
    public Battle Battle => Player.currentBattle;
    public BattleLogManager Log => BattleLogManager.instance;
    public BattleManager Hud => BattleManager.instance;
    public BattleAnimManager Anim => BattleAnimManager.instance;

    protected Canvas canvas;
    protected RectTransform canvasRect;

    [SerializeField] protected CardInfoView cardInfoView;

    protected override void Awake()
    {
        base.Awake();
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
    }

}

public class BattleCardPlaceInfo {
    public int unitId = 0;
    public BattlePlaceId place = BattlePlaceId.None;
    public int index = 0;

    public BattleCard GetBattleCard(BattleState state) {
        var unit = (unitId == 0) ? state.myUnit : state.opUnit;
        
        return place switch {
            BattlePlaceId.Deck => unit.deck.cards[index],
            BattlePlaceId.Hand => unit.hand.cards[index],
            BattlePlaceId.Leader => unit.leader.leaderCard,
            // BattlePlaceId.Territory => unit.territory,
            BattlePlaceId.Field => unit.field.cards[index],
            BattlePlaceId.Token => BattleCard.Get(index),
            _ => null,
        };
    }

    public static BattleCardPlaceInfo Parse(int code) {
        if (code < 1000)
            return new BattleCardPlaceInfo() {
                unitId = code / 100,
                place = (BattlePlaceId)(code % 100 / 10),
                index = code % 10,
            }; 
        else   
            return new BattleCardPlaceInfo() {
                unitId = 0,
                place = BattlePlaceId.Token,
                index = code,
            };
    }

    public int ToIntCode() {
        return (index > 10) ? index : (unitId * 100 + (int)place * 10 + index);
    }

}
