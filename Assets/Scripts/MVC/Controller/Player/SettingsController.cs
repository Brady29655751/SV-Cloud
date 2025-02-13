using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : IMonoBehaviour
{
    [SerializeField] private SettingsModel settingsModel;
    [SerializeField] private SettingsView settingsView;

    protected override void Awake()
    {
        base.Awake();
        settingsView.SetAnnounce(GameManager.versionData.News);
    }

    public override void Init()
    {
        base.Init();
        InitSettings();
    }

    private void InitSettings() {
        settingsView.SetName(Player.gameData.nickname);
        settingsView.SetScreenSize(Utility.GetScreenSize());
        settingsView.SetBGMVolume(Player.gameData.BGMVolume);
        settingsView.SetSEVolume(Player.gameData.SEVolume);
        settingsView.SetTurnEndHint(Player.gameData.turnEndHint);
    }

    public void SetActive(bool active) {
        gameObject.SetActive(active);
    }

    public void SetNickname() {
        void SetPlayerName(string name) {
            Player.gameData.SetNickname(name);
            settingsView.SetName(Player.gameData.nickname);
            SaveSystem.SaveData();     
        }

        InputHintbox hintbox = Hintbox.OpenHintbox<InputHintbox>();
        hintbox.SetTitle("更改暱稱");
        hintbox.SetContent("請輸入新暱稱");
        hintbox.SetNote("13個文字以內");
        hintbox.SetInputField(13);
        hintbox.SetOptionCallback(SetPlayerName);
    }

    public void SetPreviewScreenHeight(string width) {
        settingsView.SetScreenHeight(settingsModel.Height);
    }

    public void OnConfirmSettings() {
        settingsModel.OnConfirmSettings();
    }
}
