using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleUseAnimView : BattleBaseView
{
    [SerializeField] private float useSeconds = 0.25f;
    [SerializeField] private float exhibitSeconds = 0.5f;
    [SerializeField] private float exhibitScale = 0.6f;
    [SerializeField] private Vector2 exhibitPos;
    [SerializeField] private List<Image> opSleeves;
    [SerializeField] private CardView cardView;
    
    public void MeUseCard(Card card, Action callback) {
        StartCoroutine(ShowMyUsedCard(card, callback));
    }

    public void OpUseCard(Card card, Action callback) {
        StartCoroutine(ShowOpUsedCard(card, callback));
    }

    private IEnumerator ShowMyUsedCard(Card card, Action callback) {
        cardView.rectTransform.anchoredPosition = exhibitPos;
        cardView.rectTransform.localScale = exhibitScale * Vector3.one;
        cardView.SetCard(card);
        
        yield return new WaitForSeconds(exhibitSeconds);

        cardView.SetCard(null);

        callback?.Invoke();
    }

    private IEnumerator ShowOpUsedCard(Card card, Action callback) {
        float currentTime = 0, finishTime = useSeconds, percent = 0;
        var lastSleeve = opSleeves.Find(x => x.gameObject.activeSelf);
        var x = lastSleeve.rectTransform.anchoredPosition.x + 60;
        var initPos = new Vector2(x, -10);

        cardView.rectTransform.anchoredPosition = initPos;
        cardView.rectTransform.localScale = 0.3f * Vector3.one;
        cardView.SetCard(card);

        while (currentTime < finishTime) {
            percent = currentTime / finishTime;
            cardView.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.3f, exhibitScale, percent);
            cardView.rectTransform.anchoredPosition = Vector2.Lerp(initPos, exhibitPos, percent);
            currentTime += Time.deltaTime;
            yield return null;
        }

        cardView.rectTransform.anchoredPosition = exhibitPos;

        yield return new WaitForSeconds(exhibitSeconds);

        cardView.SetCard(null);

        callback?.Invoke();
    }

}
