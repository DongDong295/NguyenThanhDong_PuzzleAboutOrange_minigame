using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZBase.Foundation.Singletons;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _managerHolder;
    //private IManager _managers;
    void Awake()
    {
        InitiateManagers().Forget();
    }
    void Start()
    {
    }

    void Update()
    {

    }

    private async UniTask InitiateManagers()
    {
        var managers = _managerHolder.GetComponentsInChildren<IManager>();
        foreach (var manager in managers)
        {
            Debug.Log(manager);
            await manager.OnApplicationStart();
        }
        Singleton.Of<DataManager>().Initiate();
        Pubsub.Publisher.Scope<UIScope>().Publish(new ShowScreenEvent(Constants.UI.MAIN_MENU_SCREEN, false));
    }
}
