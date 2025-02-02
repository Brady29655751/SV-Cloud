using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeckBuilderModel : IMonoBehaviour
{
    public Deck InitDeck { get; private set; }
    public Deck CurrentDeck { get; private set; }
    public CardFilter InitFilter => GetInitFilter();
    public List<Card> InitCardStorage => CardDatabase.CardMaster.Where(InitFilter.Filter).ToList();
    public Card[] PortalResultSelections { get; private set; }

    public bool IsCurrentCardFromPortal { get; private set; }
    public Card CurrentCard { get; private set; }
    public Vector2 InitCardAnchoredPos { get; private set; }

    public CardFilter GetInitFilter() {
        CardFilter filter = new CardFilter(InitDeck.format);
        filter.zone = InitDeck.zone;
        if (InitDeck.craft == (int)CardCraft.Neutral)
            filter.craftList = CardDatabase.craftNameDict.Select(x => (int)x.Key).ToList();
        else
            filter.craftList = new List<int>() { (int)CardCraft.Neutral, InitDeck.craft };

        return filter;
    }

    public void SetInitDeck(Deck deck) {
        InitDeck = deck;
        CurrentDeck = new Deck(InitDeck);
    }

    public void SaveDeck() {
        if (CurrentDeck.CardCount == 0) {
            Hintbox.OpenHintbox("牌組至少需要放入一張牌");
            return;
        }

        if (InitDeck.IsUnnamed()) {
            var inputHintbox = Hintbox.OpenHintbox<InputHintbox>();
            inputHintbox.SetTitle("保存牌組");
            inputHintbox.SetContent("請輸入牌組名稱");
            inputHintbox.SetNote("13個文字以內");
            inputHintbox.SetInputField(13);
            inputHintbox.SetOptionCallback(OnConfirmSaveDeck);
            return;
        }

        OnConfirmSaveDeck(CurrentDeck.name);
    }

    private void OnConfirmSaveDeck(string deckName) {
        if (InitDeck.IsUnnamed()) {
            if (string.IsNullOrEmpty(deckName)) {
                Hintbox hintbox = Hintbox.OpenHintbox();
                hintbox.SetTitle("提示");
                hintbox.SetContent("牌組名稱不能為空");
                hintbox.SetOptionNum(1);
                return;
            }
            CurrentDeck.name = deckName;
            Player.gameData.decks.Add(CurrentDeck);
        } else {
            InitDeck.cardIds = new List<int>(CurrentDeck.cardIds);
        }
        SaveSystem.SaveData();
        SceneLoader.instance.ChangeScene(SceneId.Main);
    }

    public void RemoveDeck() { 
        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("放棄編輯");
        hintbox.SetContent("確定要放棄編輯嗎？\n您的變更將不會儲存。");
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(OnConfirmRemoveDeck);
    }

    private void OnConfirmRemoveDeck() {
        Player.currentDeck = null;
        SceneLoader.instance.ChangeScene(SceneId.Main);
    }

    public void AddCard() {
        CurrentDeck.AddCard(CurrentCard);
    }

    public void RemoveCard() {
        CurrentDeck.RemoveCard(CurrentCard);
    }

    public void SetPortalResultSelections(Card[] cards) {
        PortalResultSelections = cards;
    }

    public int[] GetPortalSelectionsCardCount(Card[] selections) {
        return selections.Select(x => Mathf.Max(0, Card.Get(x.NameId).CountLimit - 
            CurrentDeck.CardNameIdDistribution.Get(x.NameId, 0))).ToArray();
    }

    public void SetCardSource(bool isPortal) {
        IsCurrentCardFromPortal = isPortal;
    }

    public void SetCard(Card card) {
        CurrentCard = card;
    }

    public void SetInitCardAnchoredPos(Vector2 pos) {
        InitCardAnchoredPos = pos;
    }
}
