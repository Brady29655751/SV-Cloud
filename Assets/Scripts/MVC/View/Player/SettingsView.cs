using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsView : IMonoBehaviour
{
    [SerializeField] private Text announceText;
    [SerializeField] private Text nameText;
    [SerializeField] private IInputField widthInputField;
    [SerializeField] private Text heightText;
    [SerializeField] private Slider BGMSlider, SESlider;
    [SerializeField] private Toggle turnEndHintToggle;

    public void SetAnnounce(string announce) {
        announceText?.SetText(announce);
    }

    public void SetName(string name) {
        nameText?.SetText(name);
    }

    public void SetScreenSize(Vector2Int size) {
        SetScreenSize(size.x, size.y);
    }

    public void SetScreenSize(int width, int height) {
        SetScreenWidth(width);
        SetScreenHeight(height);
    }

    public void SetScreenWidth(int width) {
        widthInputField?.SetInputString(width.ToString());
    }

    public void SetScreenHeight(int height) {
        heightText?.SetText(height.ToString());
    }

    public void SetBGMVolume(float volume) {
        BGMSlider.value = volume;
    }

    public void SetSEVolume(float volume) {
        SESlider.value = volume;
    }

    public void SetTurnEndHint(bool isOn) {
        turnEndHintToggle.isOn = isOn;
    }

}
