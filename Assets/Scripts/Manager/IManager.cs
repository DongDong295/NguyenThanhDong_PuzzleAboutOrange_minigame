using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public interface IManager
{
    public async UniTask OnApplicationStart() { await UniTask.CompletedTask; }
}
