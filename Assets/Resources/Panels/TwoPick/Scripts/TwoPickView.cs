using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;   

public class TwoPickView : IMonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Text roomNumText;
    [SerializeField] private DeckTitleView deckTitleView;
    [SerializeField] private CardView[] leftCardViews, rightCardViews;

    public override void Init()
    {
        base.Init();
        SetRoom();
    }

    private void SetRoom() {
        roomNumText?.SetText(PhotonNetwork.CurrentRoom?.Name ?? "單人");
    }

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
