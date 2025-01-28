using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CardDetailDescriptionView : IMonoBehaviour
{
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private GameObject followerObject, spellObject;
    [SerializeField] private CardDetailDescriptionFollowerView followerView;
    [SerializeField] private CardDetailDescriptionFollowerView evolveView;
    [SerializeField] private CardDetailDescriptionSpellView spellView;

    public void SetCard(Card card, bool isStoryMode = false) {
        followerObject.SetActive((card != null) && (card.Type == CardType.Follower));
        spellObject.SetActive((card != null) && (card.Type != CardType.Follower));

        if (card == null)
            return;

        followerView.SetCard(card, isStoryMode);
        evolveView?.SetCard(card?.EvolveCard, isStoryMode);
        spellView.SetCard(card, isStoryMode);

        float followerSize = 45 + Mathf.Max(70, followerView.GetContentSize());
        float evolveSize = followerSize + 45 + evolveView.GetContentSize();
        float spellSize = spellView.GetContentSize();
        float contentSize = card.IsFollower() ? evolveSize : spellSize;

        evolveView?.SetAnchoredPos(new Vector2(0, -followerSize));
        contentRect?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentSize);
    }
}
