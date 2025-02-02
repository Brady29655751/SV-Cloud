using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleRecordManager : Manager<BattleRecordManager>
{
    private BattleManager Hud => BattleManager.instance;
    private BattleAnimManager Anim => BattleAnimManager.instance;
    private List<IKeyValuePair<int[], bool>> ActionList => Player.currentBattleRecord?.actionList;

    [SerializeField] private IButton refreshButton, playButton, pauseButton, stopButton;

    private int cursor = 0;
    public bool IsPaused { get; private set; } = false;
    public bool IsStopped { get; private set; } = false;

    public override void Init()
    {
        base.Init();
        refreshButton?.gameObject.SetActive(Player.currentBattleRecord == null);
        playButton?.gameObject.SetActive(false);
        pauseButton?.gameObject.SetActive(Player.currentBattleRecord != null);
        stopButton?.gameObject.SetActive(Player.currentBattleRecord != null);
    }

    public void StartRecord() {
        if (ListHelper.IsNullOrEmpty(ActionList))
            return;

        StartCoroutine(RecordCoroutine());
    }

    private IEnumerator RecordCoroutine() {
        yield return WaitForSeconds(4);
        while (cursor < ActionList.Count) {
            var action = ActionList[cursor];
            Player.currentBattle.PlayerAction(action.Key, action.Value);
            yield return WaitForCondition(() => !Hud.IsLocked);
            yield return WaitForSeconds(1f);
            yield return WaitForCondition(() => (!IsPaused) || IsStopped);

            if (IsStopped) {
                refreshButton?.gameObject.SetActive(true);
                Hud.Refresh();
                yield break;
            }
            cursor++;
        }
        Anim.ResultAnim("DRAW", "其中一方連線中斷", () => Hud.BackToScene());
    }

    public void Play() {
        IsPaused = false;
        playButton?.gameObject.SetActive(false);
        pauseButton?.gameObject.SetActive(true);
    }

    public void Pause() {
        IsPaused = true;
        playButton?.gameObject.SetActive(true);
        pauseButton?.gameObject.SetActive(false);
    }

    public void Stop() {
        IsStopped = true;
        playButton?.gameObject.SetActive(false);
        pauseButton?.gameObject.SetActive(false);
        stopButton?.gameObject.SetActive(false);
    }

}
