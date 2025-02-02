using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class BattleAttackAnimView : BattleBaseView
{
    [SerializeField] private float attackSeconds = 0.5f;
    [SerializeField] private float attackRotationY = 90;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private BattleLeaderView myLeaderView, opLeaderView;
    [SerializeField] private List<BattleCardView> myCardViews;
    [SerializeField] private List<BattleCardView> opCardViews;

    private Vector2 startPoint, endPoint;
    private List<int> targetIndexResult = new List<int>();

    public void Attack(int unitId, int index) {
        StartCoroutine(AttackCoroutine(unitId, index));
    }

    private IEnumerator AttackCoroutine(int unitId, int index) {
        float currentTime = 0, finishTime = attackSeconds, percent = 0;
        var fieldView = (unitId == 0) ? myCardViews : opCardViews;
        // var pos = fieldView[index].rectTransform.anchoredPosition3D;

        while (currentTime < finishTime) {
            percent = currentTime / finishTime;
            var rotationY = (percent < 0.5f) ? Mathf.Lerp(0, attackRotationY, percent * 2) : Mathf.Lerp(attackRotationY, 0, 2 * percent - 1);
            fieldView[index].rectTransform.localRotation = Quaternion.Euler(0, rotationY, 0);
            currentTime += Time.deltaTime;
            yield return null;
        }

        var pos = fieldView[index].rectTransform.anchoredPosition3D;
        fieldView[index].rectTransform.localRotation = Quaternion.identity;
        fieldView[index].rectTransform.anchoredPosition3D = new Vector3(pos.x, pos.y, 0);
    }

    public void OnBeginDrag(int index) {
        var myUnit = Hud.CurrentState.myUnit;
        var opUnit = Hud.CurrentState.opUnit;

        var fieldCount = myUnit.field.Count;
        var myCard = myUnit.field.cards[index];

        if (Hud.IsLocked || (!myCard.CurrentCard.IsFollower()))
            return;
        
        startPoint = GetFollowerPoint(index, fieldCount, -60);

        var pos = myCardViews[index].rectTransform.anchoredPosition3D;
        myCardViews[index].rectTransform.anchoredPosition3D = new Vector3(pos.x, pos.y, -50);
        myCardViews[index].SetOutlineColor(ColorHelper.target);

        targetIndexResult = opUnit.field.GetAttackableTargetIndex(myCard, myUnit);
        for (int i = 0; i < targetIndexResult.Count; i++) {
            var targetIndex = targetIndexResult[i];
            if (targetIndex < 0)
                continue;

            opCardViews[targetIndex].SetOutlineColor(ColorHelper.target);
        }
    }

    public void OnDrag(int index) {
        var myUnit = Hud.CurrentState.myUnit;
        var myCard = myUnit.field.cards[index];

        if (Hud.IsLocked || (!myCard.CurrentCard.IsFollower()))
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, Camera.main, out endPoint);

        var distance = Vector3.Distance(startPoint, endPoint); 
        int pointCount = Mathf.Max(1, (int)(distance / 32));
        var points = Enumerable.Range(0, pointCount).Select(x => x * 1f / Mathf.Max(1, pointCount - 1)).Select(x => Vector3.Lerp(startPoint, endPoint, x)).ToArray();

        lineRenderer.positionCount = pointCount;
        lineRenderer.SetPositions(points);

        var isLeaderPointing = RectTransformUtility.RectangleContainsScreenPoint(opLeaderView.rectTransform, Input.mousePosition, Camera.main);

        for (int i = 0; i < targetIndexResult.Count; i++) {
            var targetIndex = targetIndexResult[i];
            
            if (targetIndex < 0) {
                opLeaderView.SetTargeting(isLeaderPointing);
                continue;
            }
            
            var isPointing = (!isLeaderPointing) && RectTransformUtility.RectangleContainsScreenPoint(opCardViews[targetIndex].rectTransform, Input.mousePosition, Camera.main);

            var xy = opCardViews[targetIndex].rectTransform.anchoredPosition;
            var z = isPointing ? -50 : 0;

            opCardViews[targetIndex].rectTransform.anchoredPosition3D = new Vector3(xy.x, xy.y, z);
            opCardViews[targetIndex].SetTargeting(isPointing);
        }
    }

    public void OnEndDrag(int index) {
        var myField = Hud.CurrentState.myUnit.field.cards;
        var opField = Hud.CurrentState.opUnit.field.cards;

        if (Hud.IsLocked || (!myField[index].CurrentCard.IsFollower()))
            return;
        
        lineRenderer.positionCount = 0;
        lineRenderer.SetPositions(new Vector3[] {});

        int target = int.MinValue;
        var isLeaderPointing = RectTransformUtility.RectangleContainsScreenPoint(opLeaderView.rectTransform, Input.mousePosition, Camera.main);

        for (int i = 0; i < targetIndexResult.Count; i++) {
            var targetIndex = targetIndexResult[i];

            if (targetIndex < 0) {
                opLeaderView.SetTargeting(false);

                if (isLeaderPointing)
                    target = targetIndex;

                continue;
            }

            var isPointing = (!isLeaderPointing) && RectTransformUtility.RectangleContainsScreenPoint(opCardViews[targetIndex].rectTransform, Input.mousePosition, Camera.main);

            var xy = opCardViews[targetIndex].rectTransform.anchoredPosition;
            opCardViews[targetIndex].rectTransform.anchoredPosition3D = new Vector3(xy.x, xy.y, 0);
            opCardViews[targetIndex].SetBattleCard(opField[targetIndex]);
            opCardViews[targetIndex].SetTargeting(false);

            if (isPointing)
                target = targetIndex;
        }

        startPoint = endPoint = Vector3.zero;

        if (!target.IsInRange(-1, opField.Count)) {
            var pos = myCardViews[index].rectTransform.anchoredPosition3D;
            myCardViews[index].rectTransform.anchoredPosition3D = new Vector3(pos.x, pos.y, 0);
            myCardViews[index].SetBattleCard(myField[index]);
            return;
        }

        Battle.PlayerAction(new int[] { (int)EffectAbility.Attack, index, target }, true);
    }

    private Vector2 GetFollowerPoint(int index, int fieldCount, float y = -60) {
        int q = (fieldCount - 1) / 2;
        int r = (fieldCount - 1) % 2;
        float x = (-10) + (-60 * r) + (110 * (index - q));

        return new Vector2(x, y);
    }


}
