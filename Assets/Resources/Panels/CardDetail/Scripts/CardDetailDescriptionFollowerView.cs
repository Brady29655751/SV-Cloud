using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDetailDescriptionFollowerView : IMonoBehaviour
{
    private RectTransform rectTransform;
    [SerializeField] private Text indicator;
    [SerializeField] private GameObject splitLine;
    [SerializeField] private List<Image> atkImages = new List<Image>();
    [SerializeField] private List<Image> hpImages = new List<Image>();
    [SerializeField] private IText description;

    protected override void Awake()
    {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetCard(Card card, bool isStoryMode = false) {
        indicator?.SetText(card == null ? string.Empty : (card.Type == CardType.Evolved ? "進化後" : "進化前"));
        splitLine?.SetActive(card != null);
        description?.SetText(card == null ? string.Empty : (isStoryMode ? card.story : card.description));
        description?.SetSizeAuto(RectTransform.Axis.Vertical);

        SetStatus("atk", card == null ? -1 : card.atk);
        SetStatus("hp", card == null ? -1 : card.hp);
    }

    private void SetStatus(string status, int num) {
        var images = status switch {
            "atk" => atkImages,
            "hp" => hpImages,
            _ => null,
        };

        int ten = num % 100 / 10;
        int one = num % 10;

        images[0]?.SetColor(((ten > 0) || (num < 0)) ? Color.clear : Color.white);
        images[1]?.SetColor(((ten <= 0)|| (num < 0)) ? Color.clear : Color.white);
        images[2]?.SetColor(((ten <= 0)|| (num < 0)) ? Color.clear : Color.white);

        images[0]?.SetSprite(SpriteResources.GetCardAtkHpSprite(one));
        images[1]?.SetSprite(SpriteResources.GetCardAtkHpSprite(ten));
        images[2]?.SetSprite(SpriteResources.GetCardAtkHpSprite(one));
    }

    public void SetAnchoredPos(Vector2 pos) {
        rectTransform.anchoredPosition = pos;
    }

    public float GetContentSize() {
        return description.PreferredSize.y;
    }
}
