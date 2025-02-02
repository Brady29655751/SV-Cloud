using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class BattleMenuView : BattleBaseView
{
    [SerializeField] private Image myLeader, opLeader;
    [SerializeField] private Text myName, opName, retireText;

    private string retireStr => Player.IsRecordMode ? "退出播放" : "放棄對戰";

    public void SetActive(bool active) {
        if (active && (Hud.IsLocked || Anim.IsSelectingTarget))
            return;

        gameObject.SetActive(active);
    }

    public void InitPlayerInfo() {
        var myUnit = Battle.CurrentState.myUnit;
        var opUnit = Battle.CurrentState.opUnit;
        myName?.SetText(myUnit.name);
        opName?.SetText(opUnit.name);
        myLeader?.SetSprite(SpriteResources.GetLeaderProfileSprite(myUnit.leader.CraftId));
        opLeader?.SetSprite(SpriteResources.GetLeaderProfileSprite(opUnit.leader.CraftId));
        retireText?.SetText(retireStr);
    }

    public void Retire() {
        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle(retireStr);
        hintbox.SetContent("確定要" + retireStr + "嗎？");
        hintbox.SetOutline(Color.red);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(OnCofirmRetire);
    }

    private void OnCofirmRetire() {
        if (Player.IsRecordMode) {
            SceneLoader.instance.ChangeScene(SceneId.Main);
            return;
        }

        gameObject.SetActive(false);
        Hud.SetLock(true);
        Battle.PlayerAction(new int[] { (int)EffectAbility.SetResult, (int)BattleResultState.Lose }, true);
    }

}
