using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomPlayerView : IMonoBehaviour
{
    [SerializeField] private bool isMe = true;
    [SerializeField] private Text nameText, victoryText, readyText;
    [SerializeField] private IButton readyButton, notReadyButton;
    [SerializeField] private DeckView deckView;


    public void SetName(string name) {
        nameText?.SetText(name);
    }

    public void SetVictory(int victory) {
        victoryText?.SetText(victory.ToString());
    }

    public void SetDeck(Deck deck) {
        deckView?.SetDeck(deck);
    }

    public void SetReady(bool isReady, Action callback = null) {
        bool ok = isMe ? SetMyReady(isReady) : SetOpReady(isReady);
        if (!ok)
            return;

        callback?.Invoke();
    }

    private bool SetMyReady(bool isReady) {
        if (Player.currentDeck.IsUnnamed())
            return false;

        readyButton?.gameObject.SetActive(!isReady);
        notReadyButton?.gameObject.SetActive(isReady);
        return true;
    }

    private bool SetOpReady(bool isReady) {
        readyText?.SetText(isReady ? "準備完成" : "準備中");
        readyText?.SetColor(isReady ? ColorHelper.gold : Color.red);
        return true;
    }
}
