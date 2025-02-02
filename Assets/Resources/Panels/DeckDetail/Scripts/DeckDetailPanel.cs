using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckDetailPanel : Panel
{
    [SerializeField] private List<GameObject> deckCopyObjects;
    [SerializeField] private DeckTitleView titleView;
    [SerializeField] private DeckDetailView detailView;
    private Deck currentDeck;
    private bool isCopyAvailable => SceneLoader.CurrentSceneId == SceneId.Main;

    public override void Init()
    {
        base.Init();
        deckCopyObjects?.ForEach(x => x?.SetActive(isCopyAvailable));
    }

    public void SetDeck(Deck deck) {
        currentDeck = deck;
        titleView?.SetDeck(deck);
        detailView?.SetDeck(deck, OpenCardDetailPanel);
    }

    private void OpenCardDetailPanel(Card card) {
        var panel = Panel.OpenPanel<CardDetailPanel>();
        panel.SetCard(card);
    }

    public void CopyDeck() {
        if (!isCopyAvailable)
            return;
            
        Player.currentDeck = new Deck((CardZone)currentDeck.zone, (GameFormat)currentDeck.format, (CardCraft)currentDeck.craft)
        {
            cardIds = new List<int>(currentDeck.cardIds),
        };
        SceneLoader.instance.ChangeScene(SceneId.DeckBuilder);
    }

    public void ShareCode() {
        if (!isCopyAvailable)
            return;

        var code = currentDeck?.Code;
        if (Deck.Decode(code) == null) {
            Hintbox.OpenHintbox("牌組不合法");
            return;
        }
        GUIUtility.systemCopyBuffer = code;
        Hintbox.OpenHintbox("已複製牌組代碼到剪貼簿");
    }
    
}
