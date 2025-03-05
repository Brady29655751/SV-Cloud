using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleLogUseView : BattleBaseView
{
    private bool isMe = false;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private IButton myButton, opButton;

    private List<LogInfoView> myBattleLogs = new List<LogInfoView>();
    private List<LogInfoView> opBattleLogs = new List<LogInfoView>();

    public void LogUse(BattleState state) {
        if (state.currentEffect.ability != EffectAbility.Use)
            return;

        SetUsedCards(state, state.myUnit);
        SetUsedCards(state, state.opUnit);
    }

    private void SetUsedCards(BattleState state, BattleUnit unit) {
        bool isMyUnit = unit.id == state.myUnit.id;
        var usedCards = unit.grave.usedCards;
        var distinctCards = unit.grave.DistinctUsedCards;
        var battleLogs = isMyUnit ? myBattleLogs : opBattleLogs;
        var color = isMyUnit ? Color.cyan : Color.red;

        if (battleLogs.Count < distinctCards.Count) {
            var obj = Instantiate(SpriteResources.Log, scrollRect.content);
            var logPrefab = obj.GetComponent<LogInfoView>();
            battleLogs.Add(logPrefab);

            obj.SetActive(isMyUnit == isMe);
        }

        for (int i = 0; i < battleLogs.Count; i++) {
            int copy = i;
            var count = usedCards.Count(x => x.id == distinctCards[i].id);
            battleLogs[i].SetEffect(distinctCards[i].name, color, state.currentEffect, () => cardInfoView?.SetCard(distinctCards[copy]));
            battleLogs[i].SetCount("x " + count);
        }
    }

    public void SetWho(bool isMe) {
        this.isMe = isMe;
        myButton.SetColor(isMe ? ColorHelper.chosen : Color.black);
        opButton.SetColor(isMe ? Color.black : ColorHelper.chosen);
        myBattleLogs.ForEach(x => x.gameObject.SetActive(isMe));
        opBattleLogs.ForEach(x => x.gameObject.SetActive(!isMe));
    }
}
