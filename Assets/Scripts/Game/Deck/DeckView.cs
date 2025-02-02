using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckView : IMonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Image leaderImage;
    [SerializeField] private IButton button;

    public void SetDeck(Deck deck) {
        button?.gameObject.SetActive(deck != null);
        if (deck == null)
            return;

        if (deck.IsDefault()) {
            nameText?.SetText(deck.name);
            leaderImage?.SetColor(Color.clear);
            button?.SetSprite(null);
            return;
        }

        var theme = SpriteResources.GetThemeBackgroundSprite(deck.craft);
        var leader = SpriteResources.GetLeaderBackgroundSprite(deck.craft);

        nameText?.SetText(deck.name);
        button?.SetSprite(theme);
        leaderImage?.SetColor(Color.white);
        leaderImage?.SetSprite(leader);
        leaderImage?.rectTransform?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, leader.GetResizedHeight(100));
    }

    public void SetButtonCallback(Action callback) {
        if (callback == null)
            return;

        button?.onPointerClickEvent?.SetListener(callback.Invoke);
    }
}
