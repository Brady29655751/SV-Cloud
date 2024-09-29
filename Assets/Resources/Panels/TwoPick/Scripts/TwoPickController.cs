using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoPickController : IMonoBehaviour
{
    [SerializeField] private TwoPickModel twoPickModel;
    [SerializeField] private TwoPickView twoPickView;

    public event Action onTwoPickComplete;

    public void InitDeck(CardZone zone, GameFormat format, CardCraft craft)
    {
        Player.currentDeck = new Deck(zone, format, craft) {
            name = craft.GetCraftName()
        };
        twoPickModel.InitDeck();
        twoPickView.SetCraft(craft);
        GetNextPair();
    }

    public void ConfirmDeck() {
        var panel = Panel.OpenPanel<DeckDetailPanel>();
        panel.SetDeck(Player.currentDeck);
    }

    public void Choose(int index) {
        twoPickModel.Choose(index);
        GetNextPair();
    }

    public void ShowCard(int index) {
        var pair = (index / 10 == 0) ? twoPickModel.leftPair : twoPickModel.rightPair;
        var panel = Panel.OpenPanel<CardDetailPanel>();
        panel.SetCard(pair[index % 10]);
    }

    private void GetNextPair() {
        twoPickModel.GetNextPair();
        if ((twoPickModel.leftPair == null) || (twoPickModel.rightPair == null)) {
            onTwoPickComplete?.Invoke();
            return;
        }
        twoPickView.SetNextPair(twoPickModel.leftPair, twoPickModel.rightPair);
    }
}
