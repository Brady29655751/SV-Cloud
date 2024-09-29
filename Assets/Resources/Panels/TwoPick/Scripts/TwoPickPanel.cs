using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoPickPanel : Panel
{
    [SerializeField] private TwoPickController twoPickController;

    protected override void Awake() {
        base.Awake();
        twoPickController.onTwoPickComplete += OnTwoPickComplete;
    }

    private void OnTwoPickComplete() {
        twoPickController.onTwoPickComplete -= OnTwoPickComplete;
        ClosePanel();
    }

    public void InitDeck(CardZone zone, GameFormat format, CardCraft craft) {
        twoPickController.InitDeck(zone, format, craft);
    }
}
