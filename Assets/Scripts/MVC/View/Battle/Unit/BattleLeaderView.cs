using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleLeaderView : BattleBaseView
{
    [SerializeField] private int id;
    [SerializeField] public RectTransform rectTransform;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image leaderImage;
    [SerializeField] private Text hpText;
    [SerializeField] private Outline hpOutline;

    [SerializeField] private BattleCardEffectView effectView;

    private Leader currentLeader;

    public void SetLeader(Leader leader) {
        currentLeader = leader;
        backgroundImage?.SetColor(SpriteResources.GetLeaderBattleColor(leader.Craft));
        leaderImage?.SetSprite(SpriteResources.GetLeaderProfileSprite(leader.CraftId));
        hpText?.SetText(leader.HP.ToString());
        hpText?.SetColor(ColorHelper.GetAtkHpTextColor(leader.HP, leader.HPMax, leader.HPInit));
        hpOutline?.SetColor(ColorHelper.GetAtkHpOutlineColor(leader.HP, leader.HPMax, leader.HPInit));
    }

    public void ShowLeaderInfo() {
        Hud.CurrentCardPlaceInfo = new BattleCardPlaceInfo() { 
            unitId = id,
            place = BattlePlaceId.Hand,
            index = 0,
        };

        cardInfoView?.SetBattleCard(currentLeader.leaderCard);
    }

    public void SetTargeting(bool isTargeting) {
        effectView?.SetTargeting(isTargeting);
    }

    public void SetDamage(int damage, Color color, Action callback) {
        effectView?.SetDamage(damage, color, callback);
    }

}
