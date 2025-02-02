using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSettings 
{
    public bool isLocal = false;
    public CardZone zone = CardZone.Engineering;
    public GameFormat format = GameFormat.Rotation;
    public int evolveStart = 4;
    public string masterName;
    public string clientName;
    public int seed = 0;

    public BattleSettings(){}
    public BattleSettings(CardZone zoneId, GameFormat formatId, bool local = false, int evolveStartTurn = 4) {
        isLocal = local;
        zone = zoneId;
        format = formatId;
        evolveStart = evolveStartTurn;
        seed = Random.Range(int.MinValue, int.MaxValue);
    }

    public BattleSettings(BattleSettings rhs) {
        isLocal = rhs.isLocal;
        zone = rhs.zone;
        format = rhs.format;
        evolveStart = rhs.evolveStart;
        masterName = rhs.masterName;
        clientName = rhs.clientName;
        seed = rhs.seed;
    }
}
