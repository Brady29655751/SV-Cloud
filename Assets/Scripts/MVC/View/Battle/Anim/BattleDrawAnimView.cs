using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleDrawAnimView : BattleBaseView
{
    [SerializeField] private float drawSeconds;
    [SerializeField] private float waitSeconds = 0.5f;
    [SerializeField] private BattleTokenView tokenView, hideTokenView;
    [SerializeField] private Vector2 myDrawInitPos;
    [SerializeField] private Vector2 opDrawInitPos;
    [SerializeField] private List<Image> opSleeves;
    [SerializeField] private Image drawSleeve;

    public void MyDrawFromDeck(bool currentHandMode, List<Card> inHand, List<Card> inGrave, Action callback) {
        callback?.Invoke();
    }

    public void OpDrawFromDeck(Action callback) {
        StartCoroutine(OpDraw(callback));
    }

    private IEnumerator OpDraw(Action callback) {
        float currentTime = 0, finishTime = drawSeconds, percent = 0;
        var lastSleeve = opSleeves.FindLast(x => !x.gameObject.activeSelf) ?? opSleeves[0];
        var targetPos = new Vector2(lastSleeve.rectTransform.anchoredPosition.x - 330, 200);

        drawSleeve.gameObject.SetActive(true);

        while (currentTime < finishTime) {
            percent = currentTime / finishTime;
            drawSleeve.rectTransform.localScale = Vector3.one * Mathf.Lerp(1, 0.8f, percent);
            drawSleeve.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(-75, -180, percent));
            drawSleeve.rectTransform.anchoredPosition = Vector2.Lerp(opDrawInitPos, targetPos, percent);
            currentTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(waitSeconds);

        drawSleeve.gameObject.SetActive(false);

        callback?.Invoke();
    }

    public void MyGetToken(bool hide, List<Card> tokens, Action callback) {
        var getTokenView = hide ? hideTokenView : tokenView;
        getTokenView?.GetTokenAnim(true, hide, tokens, callback);
    }

    public void OpGetToken(bool hide, List<Card> tokens, Action callback) {
        var getTokenView = hide ? hideTokenView : tokenView;
        getTokenView?.GetTokenAnim(false, hide, tokens, callback);
    }

    public void MyAddDeck(bool hide, List<Card> tokens, Action callback) {
        if (hide)
            StartCoroutine(AddDeck(true, callback));
        else
            tokenView?.AddDeckAnim(true, tokens, () => StartCoroutine(AddDeck(true, callback)));
    }

    public void OpAddDeck(bool hide, List<Card> tokens, Action callback) {
        if (hide)
            StartCoroutine(AddDeck(false, callback));
        else
            tokenView?.AddDeckAnim(false, tokens, () => StartCoroutine(AddDeck(false, callback)));
    }

    private IEnumerator AddDeck(bool isMe, Action callback) {
        float currentTime = 0, finishTime = waitSeconds, percent = 0;

        drawSleeve.rectTransform.anchoredPosition = Vector2.zero;
        drawSleeve.rectTransform.localScale = Vector3.one * 1.5f;
        drawSleeve.gameObject.SetActive(true);

        while (currentTime < finishTime) {
            percent = currentTime / finishTime;
            drawSleeve.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, isMe ? -95 : -75, percent));
            currentTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(waitSeconds);

        currentTime = 0; finishTime = waitSeconds; percent = 0;

        while (currentTime < finishTime) {
            percent = currentTime / finishTime;
            drawSleeve.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.5f, 1, percent);
            drawSleeve.rectTransform.anchoredPosition = Vector2.Lerp(Vector2.zero, isMe ? myDrawInitPos : opDrawInitPos, percent);
            currentTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(waitSeconds);

        drawSleeve.gameObject.SetActive(false);

        callback?.Invoke();
    }
}
