using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePPView : BattleBaseView
{
    [SerializeField] private IButton turnEndButton;
    [SerializeField] private Text ppText, ppMaxText;
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

    public void SetTurnEnd() {
        Battle.PlayerAction(new int[] { (int)EffectAbility.TurnEnd }, true);
    }
}
