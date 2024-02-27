using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class BattleManager : Manager<BattleManager>
{
    public Battle Battle => Player.currentBattle;

    [SerializeField] private BattleSystemView systemView;
    [SerializeField] private BattleUnitView myView, opView;
    [SerializeField] private PhotonView masterPhotonView, clientPhotonView;
    public PhotonView myPhotonView => PhotonNetwork.IsMasterClient ? masterPhotonView : clientPhotonView;

    private Queue<BattleState> hudQueue = new Queue<BattleState>();
    public BattleCardPlaceInfo CurrentCardPlaceInfo = null;
    public BattleState CurrentState { get; protected set; } = null;
    public bool IsDone => systemView.IsDone && myView.IsDone && opView.IsDone;
    public bool IsLocked { get; protected set; } = false;

    protected override void Awake()
    {
        base.Awake();
        CurrentState = Battle.CurrentState;
        if (Battle.Settings.isLocal)
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
        hintbox.SetOptionCallback(() => OnConfirmBattleResult(false));
    }

    public void OnLocalPlayerDisconnect(DisconnectCause disconnectCause, string failedMessage) {
        OpenDisconnectHintbox("我方連線已中斷");
        NetworkManager.instance.onDisconnectEvent -= OnLocalPlayerDisconnect;
    }

    public void OnOtherPlayerDisconnect(Photon.Realtime.Player player) {
        OpenDisconnectHintbox("對方連線已中斷");
        NetworkManager.instance.onOtherPlayerLeftRoomEvent -= OnOtherPlayerDisconnect;
    }

    public void EnemyPlayerAction(short[] data) {
        if (Battle.Settings.isLocal)
            return;

        var photonView = masterPhotonView.IsMine ? masterPhotonView : clientPhotonView;
        photonView.RPC("RPCPlayerAction", RpcTarget.Others, (object)data);
    }

    public void SetLock(bool locked) {
        if (locked == IsLocked)
            return;

        IsLocked = locked;
        myView.SetLock(IsLocked);
    }

    public void OnConfirmBattleResult(bool isBattleEnd) {
        if (Battle.Settings.isLocal || (!isBattleEnd)) {
            BackToScene();
            return;
        }

        var masterWin = (Battle.Result.masterState == BattleResultState.Win) ? 1 : 0;
        var addWin = PhotonNetwork.IsMasterClient ? masterWin : (1 - masterWin);
        var hashtable = PhotonNetwork.LocalPlayer.CustomProperties;
        hashtable["win"] = (int)hashtable["win"] + addWin;
        PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);

        var opCraftId = Battle.CurrentState.opUnit.leader.CraftId;
        if (((CardCraft)opCraftId) != CardCraft.Neutral) {
            Player.currentDeck.battles[opCraftId] += 1;
            Player.currentDeck.wins[opCraftId] += addWin;
            SaveSystem.SaveData();
        }
    }

    public void BackToScene() {
        bool isLocal = Battle.Settings.isLocal;
        var scene = isLocal ? SceneId.Main : SceneId.Room;
        SceneLoader.instance.ChangeScene(scene);
    }

    public void Refresh() {
        StopAllCoroutines();
        hudQueue.Clear();
        CurrentState = new BattleState(Battle.CurrentState) { currentEffect = Effect.None };
        SetState(CurrentState);
        ProcessQueue();
    }

    public void SetState(BattleState state) {
        var hudState = (state == null) ? null : new BattleState(state);
        hudQueue.Enqueue(hudState);
    }
    
    public void ProcessQueue() {
        if (hudQueue.Count == 0) {
            SetLock(false);
            return;
        }

        SetLock(true);

        CurrentState = hudQueue.Dequeue();
        systemView.SetState(CurrentState);
        myView.SetState(CurrentState);
        opView.SetState(CurrentState);

        StartCoroutine(WaitForCondition(() => IsDone, ProcessQueue));
    }
}
