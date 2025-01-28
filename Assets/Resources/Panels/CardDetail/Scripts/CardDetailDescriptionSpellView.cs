using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDetailDescriptionSpellView : IMonoBehaviour
{
    [SerializeField] private Text indicator;
    [SerializeField] private IText description;

    public void SetCard(Card card, bool isStoryMode = false) {
        if ((card == null) || (card.IsFollower())) {
            gameObject.SetActive(false);
            return;
        }

        gameObject?.SetActive(true);
        indicator?.SetText(card.Type.GetTypeName());
        description?.SetText(isStoryMode ? card.story : card.description);
        description?.SetSizeAuto(RectTransform.Axis.Vertical);
    }

    public float GetContentSize() {
        return description.PreferredSize.y;
    }


}
