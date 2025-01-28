using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDetailView : IMonoBehaviour
{
    [SerializeField] private CardView cardView;
    [SerializeField] private CardDetailInfoView infoView;
    [SerializeField] private CardDetailDescriptionView descriptionView;

    public void SetCard(Card card, bool isStoryMode = false) {
        cardView?.SetCard(card);
        infoView?.SetCard(card);
        descriptionView?.SetCard(card, isStoryMode);
        
    }   
}
