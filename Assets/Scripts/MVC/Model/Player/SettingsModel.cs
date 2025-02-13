using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsModel : IMonoBehaviour
{
    public int Height => Width * 9 / 16;
    public int Width => GetScreenWidth();
    public int BGMVolume => (int)BGMSlider.value;
    public int SEVolume => (int)SESlider.value;

    [SerializeField] private IInputField widthInputField;
    [SerializeField] private Slider BGMSlider, SESlider;
    [SerializeField] private Toggle turnEndHintToggle;

    public int GetScreenWidth() {
        return int.TryParse(widthInputField.InputString, out int screenWidth) ? screenWidth : 0;
    }

    public void OnConfirmSettings() {
        Player.gameData.BGMVolume = BGMVolume;
        Player.gameData.SEVolume = SEVolume;
        Player.gameData.turnEndHint = turnEndHintToggle.isOn;
        if (Width.IsWithin(400, 1920)) {
            Utility.SetScreenSize(Width, Height);
            Hintbox.OpenHintbox("已儲存設定");
        } else {
            Hintbox.OpenHintbox("設定的視窗畫面過大或過小，無法設定");
        }
        SaveSystem.SaveData();
        AudioSystem.instance.OnConfirmSettings();
    }

}
