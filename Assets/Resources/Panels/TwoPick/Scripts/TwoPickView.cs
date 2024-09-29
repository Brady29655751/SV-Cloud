using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   

public class TwoPickView : IMonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private DeckTitleView deckTitleView;
    [SerializeField] private CardView[] leftCardViews, rightCardViews;

    public void SetCraft(CardCraft craft) {
        background.SetSprite(SpriteResources.GetThemeBackgroundSprite((int)craft));
    }

    public void SetNextPair(Card[] leftPair, Card[] rightPair) {
        deckTitleView.SetDeck(Player.currentDeck);
        for (int i = 0; i < 2; i++) {
            leftCardViews[i].SetCard(leftPair[i]);
            rightCardViews[i].SetCard(rightPair[i]);
        }
    }
}
