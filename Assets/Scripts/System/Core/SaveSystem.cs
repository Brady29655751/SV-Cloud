using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Photon.Realtime;
using UnityEngine;

public static class SaveSystem
{
    public static string SavePath => Application.persistentDataPath + "/save";

    public static void SaveData() {
        SaveXML(Player.gameData, SavePath);
    }

    public static GameData LoadData() {
        GameData data = LoadXML<GameData>(SavePath);
        if (data != null)
            return data.Verify();
        
        Player.gameData = data = GameData.GetDefaultData();
        SaveData();

        Debug.Log("Save file not found in " + SavePath);
        Debug.Log("Using default data.");

        return data;
    }

    public static void SaveXML(object item, string path) {
        string XmlPath = path + ".xml";
        using (StreamWriter writer = new StreamWriter(XmlPath)) {
            XmlSerializer serializer = new XmlSerializer(item.GetType());
            serializer.Serialize(writer.BaseStream, item);
            writer.Close();
        };
    }

    public static T LoadXML<T>(string path) {
        string XmlPath = path + ".xml";
        if (!File.Exists(XmlPath))
            return default(T);
        
        using (StreamReader reader = new StreamReader(XmlPath)) {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            T deserialized = (T)serializer.Deserialize(reader.BaseStream);
            reader.Close();
            return deserialized;
        };
    }
    
}
