using System;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialog
{
    public const int DATA_COL = 3;
    public string name, content;
    public Dictionary<string, string> options = new Dictionary<string, string>();

    public string next => options.Get("next");
    public string backgroundPath => options.Get("path");
    public string backgroundColorId => options.Get("color");
    public string bgm => options.Get("bgm");

    public string Name => name?.GetDescription();
    public string Content => content?.GetDescription();
    public Sprite Background => (backgroundPath == null) ? null : ResourceManager.instance.GetSprite(backgroundPath);
    public Color BackgroundColor => StringHelper.ToColor(backgroundColorId, Color.white);
    public AudioClip BGM => (bgm == null) ? null : ResourceManager.instance.GetAudio(bgm);

    public Dialog() { }
    public Dialog(string[] data) {
        name = data[0];
        content = data[1];
        options.ParseOptions(data[2]);
    }
}
