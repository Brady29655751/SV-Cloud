using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleCardEffectView : IMonoBehaviour
{
    [SerializeField] private float vanishSeconds;
    [SerializeField] private float damageSeconds, damagePosDeltaY;
    [SerializeField] private GameObject targetObject;
    [SerializeField] private GameObject vanishObject;
    [SerializeField] private Text damageText;
    [SerializeField] private List<GameObject> effectObjects;

    public void SetBattleCard(BattleCard card) {
        if (card == null) {
            effectObjects.ForEach(x => x?.SetActive(false));
            return;
        }
        effectObjects[0]?.SetActive(card.actionController.IsKeywordAvailable(CardKeyword.Ward));
        effectObjects[1]?.SetActive(card.actionController.IsKeywordAvailable(CardKeyword.Ambush));
        effectObjects[2]?.SetActive(card.actionController.IsKeywordAvailable(CardKeyword.Pressure));
        effectObjects[3]?.SetActive(card.CurrentCard.effects.Exists(x => x.ability == EffectAbility.SetDamage));
        effectObjects[4]?.SetActive(CardDatabase.UneffectableKeywords.Any(keyword =>
            card.actionController.IsKeywordAvailable(keyword)));
        effectObjects[5]?.SetActive(card.actionController.IsKeywordAvailable(CardKeyword.Aura));
        effectObjects[6]?.SetActive(card.actionController.IsKeywordAvailable(CardKeyword.Freeze));
        effectObjects[7]?.SetActive(false);

        // If at least 3 kinds of special effects are active, 
        // replace it with "All In One" effect.
        var allEffectRange = Enumerable.Range(2, 3).ToList();
        if (allEffectRange.All(x => effectObjects[x].activeSelf)) {
            allEffectRange.ForEach(x => effectObjects[x].SetActive(false));
            effectObjects[7]?.SetActive(true);
        }
    }

    public void SetTargeting(bool isTargeting) {
        targetObject?.SetActive(isTargeting);
    }

    public void SetDamage(int damage, Color color, Action callback) {
        StartCoroutine(FlashDamage(damage, color, callback));
    }

    private IEnumerator FlashDamage(int damage, Color color, Action callback) {
        float currentTime = 0, finishTime = damageSeconds, percent = 0;
        var initPos = damageText.rectTransform.anchoredPosition;

        damage = Mathf.Clamp(Mathf.Abs(damage), 0, 99);
        damageText.SetText(damage.ToString());
        damageText.SetColor(color);

        while (currentTime < finishTime) {
            percent = currentTime / finishTime;
            damageText.rectTransform.anchoredPosition = new Vector2(initPos.x, initPos.y + (color == Color.red ? 5f : (damagePosDeltaY * percent)));
            currentTime += Time.deltaTime;
            yield return null;
        }

        damageText.SetColor(Color.clear);
        damageText.rectTransform.anchoredPosition = initPos;

        callback?.Invoke();
    }

    public void SetLeaveField(string type, Action callback) {
        switch (type) {
            default:
                callback?.Invoke();
                break;
            case "vanish":
                StartCoroutine(VanishField(callback));
                break;
        }
    }

    private IEnumerator VanishField(Action callback) {
        if (vanishObject == null) {
            callback?.Invoke();
            yield break;
        }

        vanishObject.transform.localScale = Vector3.one;
        vanishObject.SetActive(true);

        yield return new WaitForSeconds(0.15f);

        float currentTime = 0, finishTime = vanishSeconds, percent = 0;
        while (currentTime < finishTime) {
            percent = currentTime / finishTime;
            vanishObject.transform.localScale = Mathf.Lerp(1, 0, percent) * Vector3.one;
            currentTime += Time.deltaTime;
            yield return null;
        }

        vanishObject.SetActive(false);
        callback?.Invoke();
    }
}
