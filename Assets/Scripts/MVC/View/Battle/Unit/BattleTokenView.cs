using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleTokenView : BattleBaseView
{
    [SerializeField] private float waitSeconds = 0.5f;
    [SerializeField] private float getTokenSeconds = 1f;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private HorizontalLayoutGroup layoutGroup;
    [SerializeField] private List<CardView> cardViews;
    [SerializeField] private List<Image> sleeves;

    public void GetTokenAnim(bool isMe, bool hide, List<Card> tokens, Action callback) {
        var tokenCoroutine = hide ? GetHideTokenCoroutine(isMe, tokens, callback) : GetTokenCoroutine(isMe, tokens, callback);
        StartCoroutine(tokenCoroutine);
    }

    private IEnumerator GetTokenCoroutine(bool isMe, List<Card> tokens, Action callback) {
        for (int i = 0; i < cardViews.Count; i++)
            cardViews[i].SetCard((i < tokens.Count) ? tokens[i] : null);

        rectTransform.anchoredPosition = new Vector2(GetLayoutGroupPosition(415, tokens.Count), 140);
        layoutGroup.spacing = GetLayoutGroupSpacing(tokens.Count);

        yield return new WaitForSeconds(waitSeconds);

        float currentTime = 0, finishTime = getTokenSeconds, percent = 0;
        while (currentTime < finishTime) {
            percent = currentTime / finishTime;
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 140 + 360 * percent * (isMe ? -1 : 1));
            currentTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 140 + 360 * (isMe ? -1 : 1));
        
        callback?.Invoke();
    }

    private IEnumerator GetHideTokenCoroutine(bool isMe, List<Card> tokens, Action callback) {
        for (int i = 0; i < sleeves.Count; i++)
            sleeves[i].gameObject.SetActive(i < tokens.Count);

        rectTransform.anchoredPosition = new Vector2(160, 150);

        yield return new WaitForSeconds(waitSeconds);

        float currentTime = 0, finishTime = getTokenSeconds, percent = 0;
        while (currentTime < finishTime) {
            percent = currentTime / finishTime;
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 150 + 360 * percent * (isMe ? -1 : 1));
            currentTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 150 + 360 * (isMe ? -1 : 1));

        callback?.Invoke();
    }

    public void AddDeckAnim(bool isMe, List<Card> tokens, Action callback) {
        StartCoroutine(AddDeckCoroutine(isMe, tokens, callback));
    }
    
    private IEnumerator AddDeckCoroutine(bool isMe, List<Card> tokens, Action callback) {
        for (int i = 0; i < cardViews.Count; i++)
            cardViews[i].SetCard((i < tokens.Count) ? tokens[i] : null);
        
        rectTransform.anchoredPosition = new Vector2(GetLayoutGroupPosition(415, tokens.Count), 140);
        layoutGroup.spacing = GetLayoutGroupSpacing(tokens.Count);
    
        yield return new WaitForSeconds(waitSeconds);
    
        var tokenCardViews = cardViews.Take(tokens.Count).ToList();
        var initPos = tokenCardViews.Select(x => x.rectTransform.anchoredPosition).ToList();
        var middlePos = (initPos.First() + initPos.Last()) / 2;

        float currentTime = 0, finishTime = 0.15f, percent = 0;
        while (currentTime < finishTime) {
            percent = currentTime / finishTime;
            for (int i = 0; i < tokenCardViews.Count; i++)
                tokenCardViews[i].rectTransform.anchoredPosition = Vector2.Lerp(initPos[i], middlePos, percent);

            currentTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = new Vector2(GetLayoutGroupPosition(415, tokens.Count), 460);
        for (int i = 0; i < tokenCardViews.Count; i++)
            tokenCardViews[i].rectTransform.anchoredPosition = initPos[i];
    
        callback?.Invoke();
    }

    private float GetLayoutGroupPosition(float emptyPos, int count) {
        if (count < 4)
            return emptyPos - count * 50;

        if (count == 4)
            return emptyPos - 190;

        if (count == 5)
            return emptyPos - 215;

        return emptyPos - 235;
    }

    private float GetLayoutGroupSpacing(int count) {
        if (count < 5)
            return 25;

        return count switch {
            5 => 20,
            6 => 12.5f,
            7 => 6.25f,
            8 => 2.5f,
            _ => 0,
        };
    }
}
