using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : Singleton<GameManager>
{
    public bool debugMode = false; 
    
    public static string branch => "android";
    public static string gamePlatform => "Android";
    public static string fileType => ".apk";

    public static string serverUrl => instance.debugMode ? "Data/" : "https://raw.githubusercontent.com/Brady29655751/SV-Cloud/" + branch + "/";
    public static string gameUrl => "https://media.githubusercontent.com/media/Brady29655751/SV-Cloud/" + branch + "/";

    public static string versionDataUrl => serverUrl + "System/version.xml";
    public static string gameDownloadUrl => gameUrl + "Release/SVCloud_" + gamePlatform + fileType;
    
    public static VersionData versionData { get; private set; } = null;

    public GameState state {get; private set;}
    public static event Action<GameState> OnBeforeStateChanged;
    public static event Action<GameState> OnAfterStateChanged;
    protected override void Awake() {
        base.Awake();
        
        if (versionData == null)
            ChangeState(GameState.Init);
    }

    public void ChangeState(GameState newState) {
        OnBeforeStateChanged?.Invoke(newState);
        state = newState;
        
        // Debug.Log($"New State: {newState}");

        switch(newState) {
            case GameState.Init:
                GameInit();
                break;
            case GameState.Play:
                GamePlay();
                break;
            case GameState.Quit:
                GameQuit();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, "Game Manager cannot change to unknown game state.");
        }
        OnAfterStateChanged?.Invoke(newState);
    }

    private void GameInit() {
        void OnRequestSuccess(VersionData data) {
            if (data == null) {
                OnRequestFail(null);
                return;
            }
            versionData = data.Verify();
            DatabaseManager.instance.Init();
        }
        void OnRequestFail(string error) {
            versionData = new VersionData();
            RequestManager.OnRequestFail("獲取版本資料失敗，請重新啟動遊戲");
        } 

        Utility.InitScreenSizeWithRatio(16, 9);
        Player.gameData = SaveSystem.LoadData();
        ResourceManager.LoadXML<VersionData>(versionDataUrl, OnRequestSuccess, OnRequestFail);
    }

    private void GameQuit() {
        Application.Quit();
    }

    private void GamePlay() {
        
    }

}

public enum GameState {
    Init = 0,
    Play = 1,
    Quit = 2
}
