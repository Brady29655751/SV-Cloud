using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleEPView : BattleBaseView
{
    [SerializeField] private Image epContainer;
    [SerializeField] private List<Image> epTwoView;
    [SerializeField] private List<Image> epThreeView;

    public void SetLeader(Leader leader) {
        epContainer?.SetSprite(((leader.EP == 0) || (leader.isEpUsed)) ? 
            SpriteResources.EPContainerUsed : SpriteResources.EPContainer);

        for (int i = 0; i < epTwoView.Count; i++) {
            epTwoView[i].gameObject.SetActive(leader.EPMax == 2);
            epTwoView[i].SetSprite(i < leader.EP ? SpriteResources.EP : SpriteResources.EPUsed);
        }
        
        for (int i = 0; i < epThreeView.Count; i++) {
            epThreeView[i].gameObject.SetActive(leader.EPMax == 3);
            epThreeView[i].SetSprite(i < leader.EP ? SpriteResources.EP : SpriteResources.EPUsed);
        }
    }
}
