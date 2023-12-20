using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class BattleManager : Manager<BattleManager>
{
    public Battle Battle => Player.currentBattle;

    [SerializeField] private GameObject lockObject;
    [SerializeField] private BattleSystemView systemView;
    [SerializeField] private BattleUnitView myView, opView;
    [SerializeField] private PhotonView masterPhotonView, clientPhotonView;

    public PhotonView myPhotonView => PhotonNetwork.IsMasterClient ? masterPhotonView : clientPhotonView;
    public bool IsDone => systemView.isDone && myView.isDone && opView.isDone;
    private bool isProcessing = false;

    private Queue<BattleState> hudQueue = new Queue<BattleState>();

    protected override void Awake()
    {
        base.Awake();
        if (Battle.settings.isLocal)
            return;
            
        NetworkManager.instance.onDisconnectEvent += OnLocalPlayerDisconnect;
        NetworkManager.instance.onOtherPlayerLeftRoomEvent += OnOtherPlayerDisconnect;

        myPhotonView.RequestOwnership();
    }

    protected void OpenDisconnectHintbox(string message) {
        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent(message);
        hintbox.SetOptionNum(1);
        hintbox.SetOptionCallback(OnConfirmBattleResult);
    }

    public void OnLocalPlayerDisconnect(DisconnectCause disconnectCause, string failedMessage) {
        OpenDisconnectHintbox("我方連線已中斷");
        NetworkManager.instance.onDisconnectEvent -= OnLocalPlayerDisconnect;
    }

    public void OnOtherPlayerDisconnect(Photon.Realtime.Player player) {
        OpenDisconnectHintbox("對方連線已中斷");
        NetworkManager.instance.onOtherPlayerLeftRoomEvent -= OnOtherPlayerDisconnect;
    }

    public void EnemyPlayerAction(int[] data) {
        if (Battle.settings.isLocal)
            return;

        var photonView = masterPhotonView.IsMine ? masterPhotonView : clientPhotonView;
        photonView.RPC("RPCPlayerAction", RpcTarget.Others, (object)data);
    }

    [PunRPC]
    private void RPCPlayerAction(int[] data) {
        Battle.PlayerAction(data, false);
    }

    public void SetLock(bool locked) {
        lockObject?.SetActive(locked);
    }

    public void OnConfirmBattleResult() {
        var scene = Battle.settings.isLocal ? SceneId.Main : SceneId.Room;
        SceneLoader.instance.ChangeScene(scene);
    }

    public void SetState(BattleState state) {
        var hudState = (state == null) ? null : new BattleState(state);
        hudQueue.Enqueue(hudState);
    }
    
    public void ProcessQueue() {
        if (hudQueue.Count == 0) {
            isProcessing = false;   
            return;
        }

        if (isProcessing)
            return;

        var hudState = hudQueue.Dequeue();
        systemView.SetState(hudState);
        myView.SetUnit(hudState?.myUnit);
        opView.SetUnit(hudState?.opUnit);

        StartCoroutine(WaitForCondition(() => IsDone, ProcessQueue));
    }
}
