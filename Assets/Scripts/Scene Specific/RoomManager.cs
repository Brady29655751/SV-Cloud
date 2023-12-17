using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomManager : Manager<RoomManager>
{
    [SerializeField] private RoomInfoView infoView;
    [SerializeField] private RoomPlayerView myView, opView;
    [SerializeField] private DeckListController deckListController;

    protected override void Awake()
    {
        base.Awake();
        InitSubscriptions();
    }

    private void Start() {
        AudioSystem.instance.PlayMusic("Main");

        InitPlayer();
        SetMyReady(false);
    }

    private void InitSubscriptions() {
        NetworkManager.instance.onPlayerPropsUpdateEvent += OnPlayerReady;
        NetworkManager.instance.onOtherPlayerJoinedRoomEvent += OnOtherPlayerJoin;
        NetworkManager.instance.onOtherPlayerLeftRoomEvent += OnOtherPlayerLeft;

        deckListController.onUseDeckEvent += () => SetDeckListPanelActive(false);
    }

    private void RemoveSubscriptions() {
        NetworkManager.instance.onPlayerPropsUpdateEvent -= OnPlayerReady;
        NetworkManager.instance.onOtherPlayerJoinedRoomEvent -= OnOtherPlayerJoin;
        NetworkManager.instance.onOtherPlayerLeftRoomEvent -= OnOtherPlayerLeft;
    }

    private void InitPlayer() {
        var otherPlayer = PhotonNetwork.PlayerListOthers;
        var room = PhotonNetwork.CurrentRoom.CustomProperties;
        int seed = Random.Range(int.MinValue, int.MaxValue);
        int zfb = (int)room["zfb"];

        if (PhotonNetwork.IsMasterClient) {
            room["seed"] = seed;
            PhotonNetwork.CurrentRoom?.SetCustomProperties(room);
        }

        myView.SetName(PhotonNetwork.LocalPlayer.NickName);
        myView.SetDeck(Player.currentDeck);
        myView.SetVictory((int)PhotonNetwork.LocalPlayer.CustomProperties["win"]);

        opView.SetName((otherPlayer.Length == 0) ? string.Empty : otherPlayer[0].NickName);
        opView.SetVictory((otherPlayer.Length == 0) ? 0 : (int)otherPlayer[0].CustomProperties["win"]);

        deckListController.SetZone(zfb / 1000);
        deckListController.SetFormat(zfb % 1000 / 100);
    }

    public void LeaveRoom() {
        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("離開對戰室");
        hintbox.SetContent("確定要離開對戰室嗎？");
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(OnConfirmLeaveRoom);
    }

    private void OnConfirmLeaveRoom() {
        RemoveSubscriptions();
        SetMyReadyProperty(false);
        NetworkData networkData = new NetworkData() {
            networkAction = NetworkAction.Leave,
        };
        NetworkManager.instance.StartNetworkAction(networkData);
    }

    private void OnOtherPlayerJoin(Photon.Realtime.Player player) {
        opView.SetName(player.NickName);
        opView.SetVictory((int)player.CustomProperties["win"]);
    }

    private void OnOtherPlayerLeft(Photon.Realtime.Player player) {
        opView.SetName(string.Empty);
        opView.SetVictory(0);
    }

    private void OnPlayerReady(Photon.Realtime.Player player) {
        var allPlayers = PhotonNetwork.PlayerList;        
        var otherPlayer = PhotonNetwork.PlayerListOthers;
        var hash = player.CustomProperties;
        if ((!player.IsLocal) && (bool)hash["ready"]) {
            opView.SetReady(true);
        }

        bool isAllReady = (allPlayers.Length > 1) && allPlayers.All(x => (bool)x.CustomProperties["ready"]);
        if (isAllReady) {
            Battle battle = new Battle(
                PhotonNetwork.CurrentRoom.CustomProperties,
                PhotonNetwork.LocalPlayer.CustomProperties,
                PhotonNetwork.LocalPlayer.NickName,
                otherPlayer[0].CustomProperties,
                otherPlayer[0].NickName
            );
            StartBattle();
        }
    }

    private void SetMyReadyProperty(bool isReady) {
        var hash = PhotonNetwork.LocalPlayer.CustomProperties;
        hash["ready"] = isReady;

        if (!isReady) {
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            return;
        }
        
        hash["craft"] = Player.currentDeck.craft;
        hash["deck"] = Player.currentDeck.cardIds.ToArray();
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);        
        return;
    }

    public void SetMyReady(bool isReady) {
        myView.SetReady(isReady, () => SetMyReadyProperty(isReady));
    }

    public void SetDeckListPanelActive(bool active) {
        deckListController?.gameObject.SetActive(active);
        if (!active) {
            myView?.SetDeck(Player.currentDeck);
        }
    }

    public void CheckDeck() {
        if (Player.currentDeck.IsDefault()) {
            SetDeckListPanelActive(true);
            return;
        }
        var panel = Panel.OpenPanel<DeckDetailPanel>();
        panel.SetDeck(Player.currentDeck);
    }

    public void StartBattle() {
        RequestManager.OnRequestFail("battle start");
        // SceneLoader.instance.ChangeScene(SceneId.Battle, true);
    }


}
