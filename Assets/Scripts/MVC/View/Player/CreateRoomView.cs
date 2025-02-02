using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomView : IMonoBehaviour
{
    [SerializeField] private Hintbox hintbox;
    [SerializeField] private RectTransform battleRecordContentRect;

    public override void Init() {
        SetBattleRecordList();
    }

    public void CreateRoom() {
        NetworkManager.instance.onCreateOrJoinFailedEvent += OnCreateOrJoinRoomFailed;

        hintbox.SetTitle("提示");
        hintbox.SetContent("正在創建房間，請稍候");
        hintbox.SetOptionNum(1);
        hintbox.SetActive(true);
    }

    public void JoinRoom() {
        NetworkManager.instance.onCreateOrJoinFailedEvent += OnCreateOrJoinRoomFailed;
        hintbox.SetTitle("提示");
        hintbox.SetContent("正在加入房間，請稍候");
        hintbox.SetOptionNum(1);
        hintbox.SetActive(true);
    }

    private void OnCreateOrJoinRoomFailed(short code, string message) {
        NetworkManager.instance.onCreateOrJoinFailedEvent -= OnCreateOrJoinRoomFailed;

        hintbox.SetActive(false);
    }

    public void SetBattleRecordList() {
        var recordList = Player.gameData.battleRecords;
        if (ListHelper.IsNullOrEmpty(recordList))
            return;
        
        for (int i = recordList.Count - 1; i >= 0; i--) {
            Instantiate(SpriteResources.BattleRecord, battleRecordContentRect)
                ?.GetComponent<BattleRecordInfoView>()?.SetBattleRecord(recordList[i]);
        }
    }
}
