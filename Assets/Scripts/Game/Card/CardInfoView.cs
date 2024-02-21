using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
using UnityEngine.UI;

public class CardInfoView : IMonoBehaviour
{
    [SerializeField] private RectTransform backgroundRect;
    [SerializeField] private RectTransform infoRect;
    [SerializeField] private Text nameText, craftTraitText;
    [SerializeField] private GameObject costObject, atkObject, hpObject;
    [SerializeField] private IText descriptionText;
    [SerializeField] private CardInfoView evolveView;
    [SerializeField] public CardInfoButtonView buttonView;
    [SerializeField] public CardInfoEffectView effectView;

    private Text costText, atkText, hpText;
    private float normalSizeY, evolveSizeY;

    protected override void Awake()
    {
        base.Awake();
        costText = costObject?.GetComponentInChildren<Text>();
        atkText = atkObject?.GetComponentInChildren<Text>();
        hpText = hpObject?.GetComponentInChildren<Text>();
    }

    private void Update() {
        if (Input.GetMouseButton(0) && (backgroundRect != null)) {
            var camera = (SceneLoader.CurrentSceneId == SceneId.Battle) ? Camera.main : null;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(backgroundRect, Input.mousePosition, camera, out var point);
            SetActive(backgroundRect.rect.Contains(point));
        }
            
    }

    public void SetActive(bool active) {
        gameObject.SetActive(active);
    }

    public void SetCard(Card card, string additionalDescription = null) {
        SetActive(card != null);
        buttonView?.Reset();

        if (card == null) {
            descriptionText?.SetText(string.Empty);
            SetBackgroundSizeAuto();
            return;
        }
        
        string traitName = card.traits.GetTraitName();
        string craftTraitName = card.Craft.GetCraftName() + ((traitName == "-") ? string.Empty : ("/" + traitName)); 

        // Set Texts.
        nameText?.SetText(card.name);
        craftTraitText?.SetText(craftTraitName);
        craftTraitText?.SetFontSize(Mathf.Min(18, 30 - craftTraitName.Length));
        costText?.SetText(card.cost.ToString());
        atkText?.SetText(card.atk.ToString());
        hpText?.SetText(card.hp.ToString());
        descriptionText?.SetText((additionalDescription ?? string.Empty) + card.description);
        descriptionText?.Rect?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, descriptionText.PreferredSize.y);
        evolveView?.SetCard(card.EvolveCard);
        effectView?.gameObject.SetActive(false);

        // Show cost/atk/hp or not.
        costObject?.SetActive((card.Type != CardType.Leader) && (card.Type != CardType.Evolved));
        atkObject?.SetActive((card.Type == CardType.Follower) || (card.Type == CardType.Evolved));
        hpObject?.SetActive((card.Type == CardType.Follower) || (card.Type == CardType.Evolved));

        // Calculate size.
        float normalMinSizeY = (card.Type == CardType.Leader) ? 85 : 160;
        normalSizeY = Mathf.Clamp(GetDescriptionTextPreferredSize(false) + 85, normalMinSizeY, 240);
        evolveSizeY = Mathf.Clamp(GetDescriptionTextPreferredSize(true) + 50, 110, 350 - normalSizeY);
        evolveSizeY = card.IsFollower() ? evolveSizeY : 0;
        
        // Adjust view size.
        SetBackgroundSizeAuto();
        infoRect?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, normalSizeY);
        evolveView?.SetAnchoredPos(new Vector2(0, -(normalSizeY + 5)));
        evolveView?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, evolveSizeY);
        buttonView?.SetAnchoredPos(new Vector2(0, -(normalSizeY + evolveSizeY + 10)));
    }

    public void SetBattleCard(BattleCard card, Action setButtonViewFunc = null) {
        var baseCard = card?.baseCard;
        SetCard(baseCard, card?.GetConditionDescription());

        setButtonViewFunc?.Invoke();

        effectView?.SetBattleCard(card);
        effectView?.SetAnchoredPos(new Vector2(0, -(normalSizeY + evolveSizeY + buttonView.RectSize + 10)));

        SetBackgroundSizeAuto();
    }

    public float GetDescriptionTextPreferredSize(bool isEvolve = false) {
        var text = isEvolve ? evolveView?.descriptionText : descriptionText;
        return text?.PreferredSize.y ?? 0;
    }

    /// <summary>
    /// Auto adjust background size with current active button count.
    /// </summary>
    public void SetBackgroundSizeAuto() {
        if (backgroundRect == null)
            return;

        backgroundRect?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, normalSizeY + evolveSizeY + 10 
            + buttonView.RectSize + effectView.RectSize);
    }

    public void SetAnchoredPos(Vector2 pos) {
        if (infoRect == null)
            return;

        infoRect.anchoredPosition = pos;
    }

    public void SetSizeWithCurrentAnchors(RectTransform.Axis axis, float size) {
        infoRect?.SetSizeWithCurrentAnchors(axis, size);
    }
}
