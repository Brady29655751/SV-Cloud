using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDetailController : IMonoBehaviour
{
    [SerializeField] private CardDetailModel detailModel;
    [SerializeField] private CardDetailView detailView;
    [SerializeField] private PageView pageView;

    public void SetInitCard(Card card) {
        detailModel.SetInitCard(card);
        OnSetPage();
    }

    public void OnSetPage() {
        detailView?.SetCard(detailModel.CurrentCard, detailModel.IsStoryMode);
        pageView?.SetPage(detailModel.Page, detailModel.LastPage);
    }

    public void OnCardPrevPage() {
        detailModel.PrevPage();
        OnSetPage();
    }

    public void OnCardNextPage() {
        detailModel.NextPage();
        OnSetPage();
    }

    public void ToggleStoryMode() {
        detailModel.SetStoryMode(!detailModel.IsStoryMode);
        OnSetPage();
    }
}
