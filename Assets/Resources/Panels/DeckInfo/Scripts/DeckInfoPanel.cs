using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class DeckInfoPanel : Panel
{
    [SerializeField] private DeckInfoView infoView;
    public UnityEvent onDeckChangeEvent = new UnityEvent();
    public UnityEvent<Deck> onDeckUseEvent = new UnityEvent<Deck>();

    private Deck currentDeck;

    private void OnDestroy() {
        onDeckChangeEvent?.RemoveAllListeners();
        onDeckUseEvent?.RemoveAllListeners();    
    }

    public void SetDeck(Deck deck) {
        currentDeck = deck;
        infoView.SetDeck(deck);
    }

    public void SetDeckInfoMode(DeckListMode mode) {
        infoView.SetDeckInfoMode(mode);
    }

    public void RenameDeck() {
        void OnConfirmRename(string newName) {
            if (string.IsNullOrEmpty(newName))
                return;
                
            currentDeck.name = newName;
            SaveSystem.SaveData();

            SetDeck(currentDeck);
            onDeckChangeEvent?.Invoke();
        }
        var hintbox = Hintbox.OpenHintbox<InputHintbox>();
        hintbox.SetTitle("編輯牌組名稱");
        hintbox.SetContent("請輸入牌組名稱");
        hintbox.SetNote("13個文字以內");
        hintbox.SetInputField(13);
        hintbox.SetOptionCallback(OnConfirmRename);
    }

    public void CheckDeck() {
        var panel = Panel.OpenPanel<DeckDetailPanel>();
        panel.SetDeck(currentDeck);
    }

    public void EditDeck() {
        Player.currentDeck = currentDeck;
        SceneLoader.instance.ChangeScene(SceneId.DeckBuilder);
    }

    public void DeleteDeck() {
        void OnConfirmDelete() {
            Player.gameData.decks.Remove(currentDeck);
            SaveSystem.SaveData();
            onDeckChangeEvent?.Invoke();
            ClosePanel();
        }
        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("刪除牌組");
        hintbox.SetContent("確定要刪除牌組嗎？");
        hintbox.SetOutline(Color.red);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(OnConfirmDelete);
    }

    public void CopyDeck() {
        currentDeck = new Deck(currentDeck) { 
            name = string.Empty,
            battles = Enumerable.Repeat(0, 9).ToList(),
            wins = Enumerable.Repeat(0, 9).ToList(),
        };
        EditDeck();
    }

    public void UseDeck() {
        onDeckUseEvent?.Invoke(currentDeck);
        ClosePanel();
    }
}
