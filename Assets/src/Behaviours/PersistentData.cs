using System;
using System.IO;
using UnityEngine;

[Serializable]
public class PersistentData
{
    public int MasterVolume = 100;
    public int SFXVolume = 100;
    public int MusicVolume = 100;
    public int CurrentLevel = 0;

    static readonly string dataPath = Path.Combine(Application.persistentDataPath, "savefile.json");

    /// <summary>
    /// The global <see cref="PersistentData"/> object.
    /// </summary>
    public static PersistentData Global { get; private set; }

    public void Save()
    {
        string json = JsonUtility.ToJson(this);
        File.WriteAllText(dataPath, json);
    }

    static PersistentData()
    {
        if (File.Exists(dataPath)) {
            Debug.Log("Loading existing save file.");
            string json = File.ReadAllText(dataPath);

            Debug.Log($"Serialized save file is {json.Length} characters in length.");
            
            try {
                var deserialized = JsonUtility.FromJson<PersistentData>(json);
                Global = deserialized;
                Debug.Log("Successfully deserialized!");
                return;
            }
            catch (Exception ex) {
                Debug.LogError("Couldn't deserialize save file - could be malformed!");
                Debug.LogError($"{ex.GetType().Name}: {ex.Message}");
                Debug.LogError($"Content: {json}");
            }
        }

        Debug.Log("Creating new save file.");
        Global = new();
    }
}