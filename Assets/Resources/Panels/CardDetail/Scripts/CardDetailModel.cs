using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DM = DatabaseManager;

public class CardDetailModel : SelectModel<Card>
{
    public bool IsStoryMode { get; private set; } = false;
    public Card InitCard { get; private set; }
    public Card CurrentCard => Selections[0];

    public List<Card> TokenCards { get; private set; } = new List<Card>();

    public void SetInitCard(Card card) {
        IsStoryMode = false;
        InitCard = card;
        TokenCards.Clear();
        GetTokenCards(InitCard);
        SetStorage(TokenCards);
    }

    public void GetTokenCards(Card card) {
        if (TokenCards.Contains(card))
            return;

        TokenCards.Add(card);
        for (int i = 0; i < card.tokenIds.Count; i++) {
            GetTokenCards(Card.Get(card.tokenIds[i]));   
        }
    }

    public void SetStoryMode(bool storyMode) {
        IsStoryMode = storyMode;
    }
}
