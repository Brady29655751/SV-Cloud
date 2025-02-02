using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Singleton<Player>
{
    public static GameData gameData = new GameData();
    public static string Nickname => gameData.nickname;
    public static Deck currentDeck = null;
    public static Battle currentBattle = null;
    public static BattleRecord currentBattleRecord = null;
    public static bool IsRecordMode => currentBattleRecord != null;
    
    private static Dictionary<string, object> sceneData = new Dictionary<string, object>();

    protected override void OnApplicationQuit() {
        base.OnApplicationQuit();
    }    

    public static object GetSceneData(string key, object defaultReturn = null) {
        return sceneData.Get(key, defaultReturn);
    }
    public static void SetSceneData(string key, object value) {
        sceneData.Set(key, value);
    }

    public static void RemoveSceneData(string key) {
        sceneData.Remove(key);
    }

}
