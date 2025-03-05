using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleLogInfoView : BattleBaseView
{
    private bool isMe = false;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private IButton myButton, opButton;
    private List<LogInfoView> myBattleLogs = new List<LogInfoView>();
    private List<LogInfoView> opBattleLogs = new List<LogInfoView>();

    public void LogInfo(BattleState state) {
        int myCount = 0;
        int opCount = 0;
        var infoKeys = EffectDatabase.GetLeaderInfoKeys();
        var infoValues = EffectDatabase.GetLeaderInfoValues();
        for (int index = 0; index < infoKeys.Length; index++) {
            var key = infoKeys[index];
            var value = infoValues[index];
            var info = state.myUnit.leader.GetIdentifier(key);
            
            if (info > 0) {
                if (myBattleLogs.Count <= myCount) {
                    var obj = Instantiate(SpriteResources.Log, scrollRect.content);
                    var logPrefab = obj.GetComponent<LogInfoView>();
                    myBattleLogs.Add(logPrefab);
                    obj.SetActive(isMe);
                }
                myBattleLogs[myCount].SetLog(value, Color.cyan);
                myBattleLogs[myCount].SetCount(info.ToString());
                myCount++;
            }

            info = state.opUnit.leader.GetIdentifier(key);
            if (info > 0) {
                if (opBattleLogs.Count <= opCount) {
                    var obj = Instantiate(SpriteResources.Log, scrollRect.content);
                    var logPrefab = obj.GetComponent<LogInfoView>();
                    opBattleLogs.Add(logPrefab);
                    obj.SetActive(!isMe);
                }
                opBattleLogs[opCount].SetLog(value, Color.red);
                opBattleLogs[opCount].SetCount(info.ToString());
                opCount++;
            }
        }
        for (int i = myCount; i < myBattleLogs.Count; i++)
            Destroy(myBattleLogs[i].gameObject);

        for (int i = opCount; i < opBattleLogs.Count; i++)
            Destroy(opBattleLogs[i].gameObject);

        myBattleLogs = myBattleLogs.GetRange(0, myCount);
        opBattleLogs = opBattleLogs.GetRange(0, opCount);
    }

    public void SetWho(bool isMe) {
        this.isMe = isMe;
        myButton.SetColor(isMe ? ColorHelper.chosen : Color.black);
        opButton.SetColor(isMe ? Color.black : ColorHelper.chosen);
        myBattleLogs.ForEach(x => x.gameObject.SetActive(isMe));
        opBattleLogs.ForEach(x => x.gameObject.SetActive(!isMe));
    }

}
