using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DeckListController : IMonoBehaviour
{
    [SerializeField] private DeckListModel deckModel;
    [SerializeField] private DeckListView deckView;
    [SerializeField] private PageView pageView;

    public event Action<Deck> onUseDeckEvent;

    public override void Init()
    {
        base.Init();
        SetDeckList(deckModel.DefaultDeckList);
    }

    public void SetDeckList(List<Deck> deckStorage) {
        deckModel.SetStorage(deckStorage);
        FilterDeckList();
    }

    private void FilterDeckList() {
        deckModel.Filter(x => x.IsDefault() || (((CardZone)x.zone == deckModel.Zone) && ((GameFormat)x.format == deckModel.Format)));
        OnDeckListSetPage();
    }

    public void SetZoneWithoutCygames(int zoneWithoutCygames) {
        SetZone(zoneWithoutCygames + 1);
    }

    public void SetZone(int zone) {
        deckModel.SetZone(zone);
        FilterDeckList();
    }

    public void SetFormat(int format) {
        deckModel.SetFormat(format);
        FilterDeckList();
    }

    public void Select(int index) {
        if (!index.IsInRange(0, deckModel.SelectionCapacity))
            return;

        Deck deck = deckModel.Selections[index];
        if (deck == null)
            return;

        switch (deck.CreateType) 
        {
            default:
                var infoPanel = Panel.OpenPanel<DeckInfoPanel>();
                infoPanel.SetDeck(deck);
                infoPanel.SetDeckInfoMode(deckModel.mode);
                infoPanel.onDeckUseEvent.SetListener(UseDeck);
                infoPanel.onDeckChangeEvent.SetListener(() => SetDeckList(deckModel.DefaultDeckList));
                return;
            case DeckCreateType.Normal:
                var leaderPanel = Panel.OpenPanel<LeaderChoosePanel>();
                leaderPanel.SetConfirmCallback(CreateDeck);
                return;
            case DeckCreateType.Code:
                var inputHintbox = Hintbox.OpenHintbox<InputHintbox>();
                inputHintbox.SetTitle("輸入牌組代碼");
                inputHintbox.SetContent(string.Empty);
                inputHintbox.SetNote(string.Empty);
                inputHintbox.SetInputField(0);
                inputHintbox.SetOptionCallback(CreateDeckByCode);
                return;
        }
    }

    private void UseDeck(Deck deck) {
        onUseDeckEvent?.Invoke(deck);
    }

    private void CreateDeck(CardCraft craft) {
        Player.currentDeck = new Deck(deckModel.Zone, deckModel.Format, craft);
        SceneLoader.instance.ChangeScene(SceneId.DeckBuilder);
    }

    private void CreateDeckByCode(string code) {
        var deck = Deck.Decode(code);
        if (deck == null) {
            Hintbox.OpenHintbox("無效的牌組代碼");
            return;
        }
        Player.currentDeck = deck;
        SceneLoader.instance.ChangeScene(SceneId.DeckBuilder);
    }

    public void ToggleTopicDeckList() {
        deckModel.ToggleTopic();
        deckView.ToggleTopicButton(deckModel.mode == DeckListMode.Topic);
        SetDeckList(deckModel.DefaultDeckList);
    }

    public void SortDeckList() {
        Player.gameData.decks = Player.gameData.decks.OrderBy(x => (x.craft == 0) ? int.MaxValue : x.craft).ThenBy(x => x.name).ToList();
        SaveSystem.SaveData();
        Init();
    }

    private void OnDeckListSetPage() {
        deckView.SetDecks(deckModel.Selections);
        pageView?.SetPage(deckModel.Page, deckModel.LastPage);
    }

    public void OnDeckListPrevPage() {
        deckModel.PrevPage();
        OnDeckListSetPage();
    }

    public void OnDeckListNextPage() {
        deckModel.NextPage();
        OnDeckListSetPage();
    }
}

public enum DeckListMode {
    Normal = 0,
    Topic = 1,
    Battle = 2,
}
