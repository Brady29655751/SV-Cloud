using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckInfoView : IMonoBehaviour
{
    private bool winMode = true;
    private Deck currentDeck = null;

    [SerializeField] private Text nameText, craftText;
    [SerializeField] private Image leaderImage, emeblemImage;
    [SerializeField] private Text winIndicatorText, winTotalText, winRateText;
    [SerializeField] private List<Text> winCraftTexts;
    [SerializeField] private List<AmountBarView> costBarViews;
    [SerializeField] private List<GameObject> modeObjects;

    public void SetDeck(Deck deck) {
        currentDeck = deck;
        nameText?.SetText(deck.name);
        craftText?.SetText(((CardCraft)deck.craft).GetCraftName());
        leaderImage?.SetSprite(SpriteResources.GetLeaderProfileSprite(deck.craft));
        emeblemImage?.SetSprite(SpriteResources.GetCardEmblemSprite(deck.craft));
        winRateText?.SetText(Mathf.Clamp(deck.WinRate * 100, 0, 100).ToString("0.00") + " %");
        for (int i = 0; i < costBarViews.Count; i++)
            costBarViews[i].SetAmount(deck.CostDistribution[i]);

        SetWinMode(true);
    }

    private void SetWinMode(bool mode) {
        winMode = mode;
        winIndicatorText?.SetText(mode ? "勝利場次" : "對戰場次");
        winTotalText?.SetText((mode ? currentDeck.TotalWins : currentDeck.TotalBattles).ToString());
        
        for (int i = 0; i < winCraftTexts.Count; i++) {
            var winCraftNum = mode ? currentDeck.wins[i] : currentDeck.battles[i];
            winCraftTexts[i]?.SetText(winCraftNum.ToString());
        }
    }

    public void ToggleWinMode() {
        SetWinMode(!winMode);
    }

    public void SetDeckInfoMode(DeckListMode mode) {
        for (int i = 0; i < modeObjects.Count; i++) {
            modeObjects[i]?.SetActive(i == (int)mode);
        }
    }

}
