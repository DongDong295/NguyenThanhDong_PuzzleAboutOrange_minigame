using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DataManager : MonoBehaviour, IManager
{
    public async UniTask OnApplicationStart()
    {
        await UniTask.CompletedTask;
    }

    private void LoadPlayerData()
    {
    }

    private void SavePlayerData()
    {
    }
}
