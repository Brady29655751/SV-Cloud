using System;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class ResourceManager : Singleton<ResourceManager>
{
    private string buffUrl => GameManager.serverUrl + "Buffs/";
    private string cardUrl => GameManager.serverUrl + "Cards/";
    private string activityUrl => GameManager.serverUrl + "Activities/";

    public string spritePath => "Sprites/";
    public string fontPath => "Fonts/";
    public string audioPath => "Audio/";
    public string prefabPath => "Prefabs/";
    public string panelPath => "Panels/";

    public string numString => "0123456789";
    public string[] fontString => new string[]{ "Weibei", "MSJH"};
    public Dictionary<string, Object> resDict = new Dictionary<string, Object>();

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Initialize.
    private void Init() {
        InitUIResources();
    }

    private void InitUIResources() {
        for (int i = 0; i < fontString.Length; i++)
            LoadFont(fontString[i]);
        
        for (int i = 0; i < numString.Length; i++)
            LoadSprite("Card Style/cost/" + numString[i].ToString());
        
        InitAll<Sprite>("Sprites/Game/ep/ep_container", "Sprites/Game/ep/container");
        InitAll<Sprite>("Sprites/Game/frame/follower_frame", "Sprites/Game/frame/follower");
        InitAll<Sprite>("Sprites/Game/frame/amulet_frame", "Sprites/Game/frame/amulet");
    }

    // Get. If not exists in resDict, load it.
    public T Get<T>(string item) where T : Object {
        T res = (T)resDict.Get(item);
        return (res == null) ? Load<T>(item) : res;
    }
    public Sprite GetSprite(string item) {
        Sprite s = (Sprite)resDict.Get(spritePath + item);
        return (s == null) ? LoadSprite(item) : s;
    }
    public Font GetFont(string item) {
        Font f = (Font)resDict.Get(fontPath + item);
        return (f == null) ? LoadFont(item) : f;
    }
    public Font GetFont(FontOption item) {
        string fontName = fontString[(int)item];
        Font f = (Font)resDict.Get(fontPath + fontName);
        return (f == null) ? LoadFont(fontName) : f;
    }
    public AudioClip GetAudio(string item) {
        AudioClip clip = (AudioClip)resDict.Get(audioPath + item);
        return (clip == null) ? LoadAudio(item) : clip;
    }
    public GameObject GetPrefab(string item) {
        GameObject prefab = (GameObject)resDict.Get(prefabPath + item);
        return (prefab == null) ? LoadPrefab(item) : prefab;
    }
    public GameObject GetPanel(string item) {
        GameObject panel = (GameObject)resDict.Get(panelPath + item + "/Panel");
        return (panel == null) ? LoadPanel(item) : panel;
    }


    // Load and Cache the resources in resDict.
    public T Load<T>(string path, string resPath = null) where T : Object {
        T res = Resources.Load<T>(path);
        resDict.Set((resPath == null) ? path : resPath, res);
        return res;
    }
    public Sprite LoadSprite(string path) {
        return Load<Sprite>(spritePath + path);
    }
    public Font LoadFont(string path) {
        return Load<Font>(fontPath + path);
    }
    public AudioClip LoadAudio(string path) {
        return Load<AudioClip>(audioPath + path);
    }
    public GameObject LoadPrefab(string path) {
        return Load<GameObject>(prefabPath + path);
    }
    public GameObject LoadPanel(string path) {
        return Load<GameObject>(panelPath + path + "/Panel");
    }

    public static T GetXML<T>(string text) {
        if (text == null)
            return default(T);

        using (TextReader reader = new StringReader(text)) {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            T deserialized = (T)serializer.Deserialize(reader);
            reader.Close();
            return deserialized;
        };
    }

    public static T LoadXML<T>(string path, Action<T> onSuccess = null, Action<string> onFail = null) {
        if (path.StartsWith("http")) {
            void OnRequestSuccess(string text) => onSuccess?.Invoke(GetXML<T>(text));
            RequestManager.instance.Get(path, OnRequestSuccess, onFail);
            return default(T);
        }
        string text = Resources.Load<TextAsset>(path.TrimEnd(".xml"))?.text;
        if (text == null) {
            onFail?.Invoke(null);
            return default(T);
        }
        T xml = GetXML<T>(text);
        onSuccess?.Invoke(xml);
        return xml;
    }

    public static string[] GetCSV(string text) {
        if (text == null)
            return null;

        return text.Split(new char[]{',', '\n'}, System.StringSplitOptions.RemoveEmptyEntries);
    }

    public static string[] LoadCSV(string path, Action<string[]> onSuccess = null, Action<string> onFail = null) {
        if (path.StartsWith("http")) {
            void OnRequestSuccess(string text) => onSuccess?.Invoke(GetCSV(text));
            RequestManager.instance.Get(path, OnRequestSuccess, onFail);
            return null;
        }
        TextAsset textAsset = Resources.Load<TextAsset>(path.TrimEnd(".csv"));
        if (textAsset == null) {
            onFail?.Invoke(null);
            return null;
        }
        string[] csv = GetCSV(textAsset?.text);
        onSuccess?.Invoke(csv);
        return csv;
    }

    public Dictionary<int, string[]> GetDescriptionInfoDict(string[] data, int dataCol) {
        dataCol = dataCol + 1;  //! plus one for id
        int dataRow = data.Length / dataCol;  
        Dictionary<int, string[]> descDict = new Dictionary<int, string[]>();
        for (int i = 1; i < dataRow; i++) {
            int cur = dataCol * i;
            if(!int.TryParse(data[cur], out int id))
                continue;
        
            descDict.Add(id, data.SubArray(cur + 1, dataCol - 1));
        }
        return descDict;
    }   

    public Dictionary<int, T> GetInfo<T>(string[] data, int dataCol, Func<string[], int, T> constructor) where T : IIdentifyHandler {
        Dictionary<int, T> infoDict = new Dictionary<int, T>();
        int dataRow = data.Length / dataCol;
        for (int i = 1; i < dataRow; i++) {
            int cur = dataCol * i;
            if (!int.TryParse(data[cur], out _))
                continue;

            T info = constructor.Invoke(data, cur);
            infoDict.Set(info.Id, info);
        }
        return infoDict;
    }

    public void LoadEffectInfo(Action<Dictionary<int, Effect>> onSuccess = null) {
        LoadCSV(cardUrl + "effect.csv", (data) => onSuccess?.Invoke(
            GetInfo<Effect>(data, Effect.DATA_COL, (d, i) => new Effect(d, i))
        ));
    }

    public Dictionary<int, Card> GetCardInfo(Dictionary<int, Card> cardInfoDict, Dictionary<int, Effect> effectInfoDict, Dictionary<int, string[]> descInfoDict) {
        Effect GetEffect(int id) {
            var e = effectInfoDict.Get(id);
            return (e == null) ? null : new Effect(e);
        };
        
        foreach (var entry in cardInfoDict) {
            entry.Value.SetEffects(entry.Value.effectIds, GetEffect);
            entry.Value.SetDescription(descInfoDict[entry.Key]);
        }

        foreach (var entry in cardInfoDict) {
            var card = entry.Value;
            if (!card.keywords.Exists(x => (x == CardKeyword.Accelerate) || (x == CardKeyword.Crystalize)))
                continue;

            var specialDescription = string.Empty;
            for (int i = 0; i < card.effects.Count; i++) {
                var effect = card.effects[i];
                var isAccel = effect.ability == EffectAbility.Accelerate;
                var isCrystal = effect.ability == EffectAbility.Crystalize;
                if (isAccel || isCrystal) {
                    var specialType = effect.ability switch {
                        EffectAbility.Accelerate => "激奏",
                        EffectAbility.Crystalize => "結晶",
                        _ => string.Empty,
                    };
                    var specialCard = cardInfoDict.Get(int.Parse(effect.abilityOptionDict.Get("id", "-1")));
                    if (specialCard == null)
                        continue;

                    var header = "[ffbb00]" + specialType + "[-] " + specialCard.cost + "；";
                    specialDescription += (header + specialCard.description).GetDescription() + "\n------\n";
                }
            }
            card.description = specialDescription + card.description;
        }

        var basicPack = cardInfoDict.Where(x => x.Value.PackId == 100).ToList();
        foreach (var entry in basicPack) {
            var newId = entry.Key + (int)1e7;
            var newCard = new Card(entry.Value){ id = newId };
            cardInfoDict.Set(newId, newCard);            
        }

        return cardInfoDict;
    }

    public void LoadCardInfo(Action<Dictionary<int, Card>> onCardSuccess = null, Action<Dictionary<int, Effect>> onEffectSuccess = null) {
        LoadCSV(cardUrl + "info.csv", (data) => {
            var cardInfoDict = GetInfo<Card>(data, Card.DATA_COL, (d, i) => new Card(d, i));
            LoadCardEffect(cardInfoDict, onCardSuccess, onEffectSuccess);
        });
    }

    private void LoadCardEffect(Dictionary<int, Card> info, Action<Dictionary<int, Card>> onCardSuccess = null, Action<Dictionary<int, Effect>> onEffectSuccess = null) {
        LoadCSV(cardUrl + "effect.csv", (data) => {
            var effectDict = GetInfo<Effect>(data, Effect.DATA_COL, (d, i) => new Effect(d, i));
            onEffectSuccess?.Invoke(effectDict);

            LoadCardDescription(info, effectDict, onCardSuccess);
        });
    }

    private void LoadCardDescription(Dictionary<int, Card> info, Dictionary<int, Effect> effect, Action<Dictionary<int, Card>> onCardSuccess = null) {
        LoadCSV(cardUrl + "description.csv", (data) => {
            var descInfoDict = GetDescriptionInfoDict(data, Card.DESC_COL);
            onCardSuccess?.Invoke(GetCardInfo(info, effect, descInfoDict));
        });
    }

    public void LoadTraitInfo(Action<Dictionary<int, string[]>> onSuccess = null) {
        LoadCSV(cardUrl + "trait.csv", (data) => onSuccess?.Invoke(GetDescriptionInfoDict(data, 1)));
    }

    public void LoadKeywordInfo(Action<Dictionary<int, string[]>> onSuccess = null) {
        LoadCSV(cardUrl + "keyword.csv", (data) => onSuccess?.Invoke(GetDescriptionInfoDict(data, 3)));
    }

    private T[] InitAll<T>(string path, string key) where T : Object {
        T[] items = Resources.LoadAll<T>(path);
        items = items.OrderBy(x => int.Parse(x.name)).ToArray();
        for (int i = 0; i < items.Length; i++) {
            resDict.Set(key + "/" + i.ToString(), items[i]);
        }
        return items;
    }

}
