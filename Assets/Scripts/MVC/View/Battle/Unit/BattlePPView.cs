using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattlePPView : BattleBaseView
{
    [SerializeField] private IButton turnEndButton;
    [SerializeField] private Text recordTurnText, ppText, ppMaxText;
    [SerializeField] private List<Image> ppOrbImages;

    public void SetLeader(Leader leader) {
        ppText?.SetText(leader.PP.ToString());
        ppMaxText?.SetText(leader.PPMax.ToString());

        if (ppOrbImages == null)
            return;

        for (int i = 0; i < ppOrbImages.Count; i++) {
            ppOrbImages[i].gameObject.SetActive(i < leader.PPMax);
            ppOrbImages[i].SetSprite((i < leader.PP) ? SpriteResources.PP : SpriteResources.PPUsed);
        }
    }

    public void SetTurnEndButtonActive(bool active) {
        turnEndButton?.gameObject.SetActive(active);
    }

    public void SetTurnEndText(string text) {
        recordTurnText?.SetText(text);
    }

    public void SetTurnEnd() {
        if (Hud.IsLocked || Anim.IsSelectingTarget)
            return;

        var hint = Player.gameData.turnEndHint;
        var myUnit = Battle.CurrentState.myUnit;
        
        if (!hint) {
            TurnEnd();
            return;
        }

        if (myUnit.hand.cards.Exists(x => x.IsUsable(myUnit))) {
            var hintbox = Hintbox.OpenHintbox("尚有可使用的卡片，確定要結束回合嗎？");
            hintbox.SetOptionNum(2);
            hintbox.SetOptionCallback(TurnEnd);
            return;
        }

        if (myUnit.field.cards.Exists(x => x.IsAttackable(myUnit))) {
            var hintbox = Hintbox.OpenHintbox("尚有可進行攻擊的從者，確定要結束回合嗎？");
            hintbox.SetOptionNum(2);
            hintbox.SetOptionCallback(TurnEnd);
            return;
        }

        TurnEnd();
    }

    private void TurnEnd() {
        Battle.PlayerAction(new int[] { (int)EffectAbility.TurnEnd }, true);
    }
}
