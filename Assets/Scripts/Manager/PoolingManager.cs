using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZBase.Foundation.Pooling.AddressableAssets;

public class PoolingManager : MonoBehaviour
{
    private readonly Dictionary<string, AddressGameObjectPool> _dictionary = new();

    public AddressGameObjectPool GetPool(string source)
    {
        if (string.IsNullOrEmpty(source))
        {
            throw new ArgumentNullException(nameof(source));
        }

        var sourceName = source.Split('.');

        if (this._dictionary.TryGetValue(sourceName[0], out AddressGameObjectPool pool))
        {
            return pool;
        }

        pool = Create(source, this.transform);

        this._dictionary.Add(source, pool);

        return pool;
    }

    public async UniTask<GameObject> Rent(string source, bool isActive = true, CancellationToken token = default)
    {
        AddressGameObjectPool pool = GetPool(source);
        var gameObject = await pool.Rent(token);
        gameObject.transform.SetParent(null);
        gameObject.name = source;
        gameObject.SetActive(isActive);
        return gameObject;
    }

    public void Return(GameObject instance)
    {
        AddressGameObjectPool pool = GetPool(instance.name);
        pool.Return(instance);
    }

    public void ReleaseInstances(string source, int keep, Action<GameObject> onReleased = null)
    {
        AddressGameObjectPool pool = GetPool(source);

        pool.ReleaseInstances(keep, onReleased);
    }

    public static AddressGameObjectPool Create(string source, Transform parent = null)
    {
        return new AddressGameObjectPool(new AddressGameObjectPrefab
        {
            Source = source,
            Parent = parent ? parent : new GameObject(source).transform
        });
    }
}
