using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _managerHolder;
    private IManager _managers;
    void Awake()
    {
        InitiateManagers();
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void InitiateManagers()
    {
        var managers = _managerHolder.GetComponentsInChildren<IManager>();
        foreach (var manager in managers)
        {
            manager.OnApplicationStart();
        }
    }
}
