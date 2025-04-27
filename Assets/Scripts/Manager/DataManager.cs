using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DataManager
{
    public Dictionary<int, int> PlayerLevelData;
    public Dictionary<string, ScriptableObject> Data;

    public void Initiate()
    {
        Data = new();
        LoadLevelData();
        Pubsub.Subscriber.Scope<GameplayScope>().Subscribe<OnFinishLevel>(UpdateLevelData);
    }

    public async UniTask<T> Load<T>(string assetName) where T : ScriptableObject
    {
        if (Data.TryGetValue(assetName, out ScriptableObject data))
        {
            return data as T;
        }
        return await Preload<T>(assetName);
    }

    public async UniTask<T> Preload<T>(string assetName) where T : ScriptableObject
    {
        T data = await Addressables.LoadAssetAsync<T>(assetName);

        Data.Replace(assetName, data);

        return data;
    }

    public void SaveLevelBoardData(int index, List<Puzzle> puzzles, List<Block> blocks)
    {
        LevelBoardData data = new LevelBoardData();

        data.puzzlePositions = new List<Vector2>();
        foreach (var p in puzzles)
        {
            data.puzzlePositions.Add(p.Position);
        }

        data.blockPositions = new List<Vector2>();
        foreach (var b in blocks)
        {
            data.blockPositions.Add(b.Position);
        }
        string json = JsonUtility.ToJson(data);
        Debug.Log("Data Saved! " + json);
        PlayerPrefs.SetString($"level-{index}", json);
        PlayerPrefs.Save();
    }

    public void UpdateLevelData(OnFinishLevel e)
    {
        if (PlayerLevelData.ContainsKey(e.LevelIndex))
        {
            if (e.Stars > PlayerLevelData[e.LevelIndex])
                PlayerLevelData[e.LevelIndex] = e.Stars;
        }
        else
        {
            PlayerLevelData.Add(e.LevelIndex, e.Stars);
        }
        int highestLevel = PlayerLevelData.Keys.Max();
        if (e.LevelIndex == highestLevel)
        {
            int nextLevel = highestLevel + 1;
            if (!PlayerLevelData.ContainsKey(nextLevel))
            {
                PlayerLevelData.Add(nextLevel, 0);
            }
        }
        SaveLevelData();
    }

    public void SaveLevelData()
    {
        string data = JsonConvert.SerializeObject(PlayerLevelData);
        PlayerPrefs.SetString("level-data", data);
        PlayerPrefs.Save();
    }

    public void LoadLevelData()
    {
        var stringData = PlayerPrefs.GetString("level-data", "{}");
        if (stringData == "{}")
        {
            PlayerLevelData = new Dictionary<int, int>();
            PlayerLevelData.Add(1, 0);
        }
        else
        {
            PlayerLevelData = JsonConvert.DeserializeObject<Dictionary<int, int>>(stringData);
        }
    }
}

public static class DictionaryExtensions
{
    public static void Replace<TKey, TValue>(this Dictionary<TKey, TValue> original, TKey key, TValue value)
    {
        if (original.ContainsKey(key))
        {
            original[key] = value;
        }
        else
        {
            original.Add(key, value);
        }
    }
}