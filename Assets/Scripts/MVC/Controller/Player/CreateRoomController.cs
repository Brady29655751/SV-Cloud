using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateRoomController : IMonoBehaviour
{
    [SerializeField] private CreateRoomModel createModel;
    [SerializeField] private CreateRoomView createView;

    public void CreateRoom() {
        createView.CreateRoom();
        createModel.CreateRoom();
    }

    public void JoinRoom() {
        if (string.IsNullOrEmpty(createModel.roomNum))
            return;
            
        createView.JoinRoom();
        createModel.JoinRoom();
    }

    //! Currently for debug battle.
    public void WatchRoom() {
        //if (!GameManager.instance.debugMode) 
        //    return;

        ResourceManager.LoadCSV(GameManager.serverUrl + "System/deckTest.csv", PrepareBattle, Debug.Log);
    }

    private void PrepareBattle(string[] deckTestData) {
        BattleSettings settings = new BattleSettings(CardZone.Engineering, GameFormat.Rotation, true) {
            evolveStart = 1,
            masterName = Player.Nickname,
            clientName = "電腦",
        };

        BattleDeck myDeck = new BattleDeck(1, 1, 1, deckTestData[0].ToIntList('/').ToArray());
        BattleDeck opDeck = new BattleDeck(1, 1, 2, deckTestData[1].ToIntList('/').ToArray());
        Battle battle = new Battle(myDeck, opDeck, settings);
        SceneLoader.instance.ChangeScene(SceneId.Battle);
    }
}
