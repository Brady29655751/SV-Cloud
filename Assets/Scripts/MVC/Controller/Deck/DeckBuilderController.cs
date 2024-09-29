using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeckBuilderController : IMonoBehaviour
{

    [SerializeField] private float addHeightThreshold, removeHeightThreshold;
    [SerializeField] private DeckBuilderModel deckBuilderModel;
    [SerializeField] private DeckTitleView deckTitleView;
    [SerializeField] private DeckDetailView deckDetailView;
    [SerializeField] private PortalResultView portalResultView;
    [SerializeField] private CardInfoView cardInfoView;

    [SerializeField] private PortalSearchController searchController;
    [SerializeField] private PortalResultController resultController;

    protected override void Awake()
    {
        base.Awake();
        resultController.onResultSetPageEvent += deckBuilderModel.SetPortalResultSelections;
        resultController.onResultSetPageEvent += SetPortalSelectionsCount;
    }


    public void SetInitDeck(Deck deck) {
        deckBuilderModel.SetInitDeck(deck);
        resultController.SetStorage(deckBuilderModel.InitCardStorage);
        searchController.SetZone(deck.zone);
        searchController.Search();   
        OnSetDeck();
    }

    public void SaveDeck() {
        deckBuilderModel.SaveDeck();
    }

    public void RemoveDeck() {
        deckBuilderModel.RemoveDeck();
    }

    public void CheckDeck() {
        var panel = Panel.OpenPanel<DeckDetailPanel>();
        panel.SetDeck(deckBuilderModel.CurrentDeck);
    }

    public void OnSetDeck() {
        deckTitleView?.SetDeck(deckBuilderModel.CurrentDeck);
        deckDetailView?.SetDeck(deckBuilderModel.CurrentDeck, SelectDeckSelections,
            OnCardBeginDrag, null, OnCardEndDrag);

        SetPortalSelectionsCount(deckBuilderModel.PortalResultSelections);
    }

    public void SelectPortalSelections(int index) {
        if (!index.IsInRange(0, deckBuilderModel.PortalResultSelections.Length))
            return;
        
        var card = deckBuilderModel.PortalResultSelections[index];
        cardInfoView?.SetCard(card);
        deckBuilderModel.SetCard(card);
        deckBuilderModel.SetCardSource(true);
    }

    public void SelectDeckSelections(Card card) {
        cardInfoView?.SetCard(card);
        deckBuilderModel.SetCard(card);
        deckBuilderModel.SetCardSource(false);
    }

    public void SetPortalSelectionsCount(Card[] cards) {
        var counts = deckBuilderModel.GetPortalSelectionsCardCount(cards);
        resultController.SetPortalSelectionsCardCount(counts);
    }

    public void OnCardBeginDrag(RectTransform rectTransform) {
        cardInfoView?.SetCard(null);
        deckBuilderModel.SetInitCardAnchoredPos(rectTransform.anchoredPosition);
    }

    public void OnCardEndDrag(RectTransform rectTransform) {
        bool scrollToCard = false;
        if (deckBuilderModel.IsCurrentCardFromPortal) {
            if (rectTransform.anchoredPosition.y > addHeightThreshold) {
                deckBuilderModel.AddCard();
                scrollToCard = true;
            }
        } else {
            if (rectTransform.anchoredPosition.y < removeHeightThreshold)
                deckBuilderModel.RemoveCard();
        }
        rectTransform.anchoredPosition = deckBuilderModel.InitCardAnchoredPos;

        OnSetDeck();
        if (scrollToCard) {
            var deck = deckBuilderModel.CurrentDeck.DistinctCards;
            int index = deck.IndexOf(deckBuilderModel.CurrentCard);
            if (index == -1)
                return;

            deckDetailView.ScrollTo(new Vector2(index * 1f / deck.Count, 0.5f));
        }
    }
 
}
