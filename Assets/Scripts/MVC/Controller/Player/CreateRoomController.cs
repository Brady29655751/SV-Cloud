using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

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
        // Panel.OpenPanel<DialogPanel>().SetStory("Data/Story/svd.csv");
        ResourceManager.LoadCSV(GameManager.serverUrl + "System/deckTest.csv", PrepareBattle, Debug.Log);
    }

    private void PrepareBattle(string[] deckTestData) {
        BattleSettings settings = new BattleSettings(CardZone.Engineering, GameFormat.Rotation, true) {
            evolveStart = 1,
            masterName = Player.Nickname,
            clientName = "電腦",
        };

        var isRealTest = false;
        var realDeck = Player.gameData.decks[0];
        var testDeck = new Deck(CardZone.Engineering, GameFormat.Rotation, CardCraft.Elf) { cardIds = deckTestData[0].ToIntList('/') };
        Deck myDeck = isRealTest ? realDeck : testDeck;
        Deck opDeck = new Deck(CardZone.Engineering, GameFormat.Rotation, CardCraft.Royal) { cardIds = deckTestData[1].ToIntList('/') };
        Battle battle = new Battle(myDeck, opDeck, settings);
        SceneLoader.instance.ChangeScene(SceneId.Battle);
    }
}
