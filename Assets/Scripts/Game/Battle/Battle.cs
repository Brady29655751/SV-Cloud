using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;
using System;

public class Battle
{
    public BattleManager Hud => BattleManager.instance;
    public BattleSettings Settings => CurrentState?.settings;
    public BattleResult Result => CurrentState?.result;
    public BattleState CurrentState;

    public Queue<Effect> effectQueue = new Queue<Effect>();
    public Queue<Tuple<int[], bool>> playerActionQueue = new Queue<Tuple<int[], bool>>();

    public Battle() {}

    public Battle(Deck masterDeck, Deck clientDeck, BattleSettings settings) {
        if (GameManager.instance.debugMode && (Player.currentBattleRecord == null))
            RecordBattle(masterDeck, clientDeck, settings, true);
        
        Init(masterDeck, clientDeck, settings);
    }

    /// <summary>
    /// Create battle from Photon Network Hashtable for PVP.
    /// </summary>
    /// <param name="roomHash">Room properties</param>
    /// <param name="myHash">Local Player properties</param>
    /// <param name="opHash">Opponent properties</param>
    public Battle(Hashtable roomHash, Hashtable myHash, string myName, Hashtable opHash, string opName) {
        var isMaster = PhotonNetwork.IsMasterClient;
        int zfb = (int)roomHash["zfb"];
        int zone = zfb / 100;
        int format = zfb % 100 / 10;
        var settings = new BattleSettings((CardZone)zone, (GameFormat)format) {
            masterName = isMaster ? myName : opName,
            clientName = isMaster ? opName : myName,
            seed = (int)roomHash["seed"],
        };

        var masterHash = isMaster ? myHash : opHash;
        var clientHash = isMaster ? opHash : myHash;
        var masterDeck = new Deck(settings.zone, settings.format, (CardCraft)((int)masterHash["craft"])){ cardIds = ((int[])masterHash["deck"]).ToList() };
        var clientDeck = new Deck(settings.zone, settings.format, (CardCraft)((int)clientHash["craft"])){ cardIds = ((int[])clientHash["deck"]).ToList() };
        
        RecordBattle(masterDeck, clientDeck, settings, isMaster);
        Init(masterDeck, clientDeck, settings);
    }

    private void RecordBattle(Deck masterDeck, Deck clientDeck, BattleSettings settings, bool isMaster) {
        BattleRecord record = new BattleRecord() {
            isMaster = isMaster,
            settings = settings,
            masterDeck = masterDeck,
            clientDeck = clientDeck,
            date = DateTime.Now,
        };
        Player.gameData.battleRecords.Add(record);
        if (Player.gameData.battleRecords.Count > 30)
            Player.gameData.battleRecords.RemoveAt(0);

        SaveSystem.SaveData();        
    }

    private void Init(Deck masterDeck, Deck clientDeck, BattleSettings settings) {
        //! BattleDeck uses Shuffle that needs Random.
        Random.InitState(settings.seed);

        var masterBattleDeck = new BattleDeck(masterDeck);
        var clientBattleDeck = new BattleDeck(clientDeck);

        this.CurrentState = new BattleState(masterBattleDeck, clientBattleDeck, settings);
        Player.currentBattle = this;
    }

    /// <summary>
    /// Player do actions, and will send RPC to notify other players IF "isMe = true". <br/>
    /// Same action should call this with isMe set to either true or false. <br/><br/>
    /// Eg. Player A draw 1 card. <br/>
    ///     "Draw" on side A deals with effect (Draw, isMe = true), <br/>
    ///     and A will then send RPC to B with (Draw, isMe = false), <br/>
    ///     "Draw" on side B deals with effect (Draw, isMe = false), <br/>
    /// </summary>
    public void PlayerAction(int[] data, bool isMe) {

        // Enqueue action when still handling effects so that action order is preserved.
        if (effectQueue.Count > 0) {
            playerActionQueue.Enqueue(new Tuple<int[], bool>(data, isMe));
            return;
        }

        if (isMe) {
            int[] enemyData = data.ToArray();
            var ability = (EffectAbility)data[0];

            if (ability.IsTargetSelectableAbility()) {
                for (int i = 2; i < data.Length; i++)
                    enemyData[i] = data[i].IsWithin(1, 999) ? ((1 - (data[i] / 100)) * 100 + data[i] % 100) : data[i];
            }
            Hud.EnemyPlayerAction(enemyData);
        }

        var leader = (isMe ? CurrentState.myUnit : CurrentState.opUnit).leader.leaderCard;
        Effect effect = new Effect(data);
        effect.source = leader;
        effect.invokeUnit = isMe ? CurrentState.myUnit : CurrentState.opUnit;
        EnqueueEffect(effect);

        if ((!Settings.isLocal) || GameManager.instance.debugMode) {
            Player.gameData.battleRecords?.LastOrDefault()?.AddAction(data, isMe);
            SaveSystem.SaveData();
        }

        if (effectQueue.Count == 1)
            ProcessQueue();
    }

    public void ProcessQueue() {
        while (effectQueue.Count > 0) {
            if (CurrentState.result.masterState != BattleResultState.None) {
                Hud.ProcessQueue();
                return;
            }
            var e = effectQueue.Peek();
            e.Apply(CurrentState);
            effectQueue.Dequeue();
        }
        Hud.ProcessQueue();

        if (playerActionQueue.Count > 0) {
            var action = playerActionQueue.Dequeue();
            PlayerAction(action.Item1, action.Item2);
        }
    }

    public void EnqueueEffect(Effect effect) {
        effectQueue.Enqueue(effect);
    }

    public void EnqueueEffect(List<Effect> effects) {
       for (int i = 0; i < effects.Count; i++) {
            EnqueueEffect(effects[i]);
        }
    }
}
