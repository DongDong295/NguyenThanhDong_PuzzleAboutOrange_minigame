using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using ZBase.Foundation.Singletons;

public class GameplayScreen : ZBase.UnityScreenNavigator.Core.Screens.Screen
{
    [SerializeField] private TextMeshProUGUI _timeDisplay;
    [SerializeField] private Button _homeButton;
    [SerializeField] private Button _resetButton;

    private LevelManager _manager;

    public override UniTask Initialize(Memory<object> args)
    {
        _manager = SingleBehaviour.Of<LevelManager>();
        _homeButton.onClick.AddListener(() =>
        {
            Pubsub.Publisher.Scope<UIScope>().Publish(new ShowScreenEvent(Constants.UI.MAIN_MENU_SCREEN, false));
            _manager.UnloadLevelObjects();
        });
        return base.Initialize(args);
    }
    public override void DidPushEnter(Memory<object> args)
    {
        var data = _manager.GetCurrentLevelData();
        _resetButton.onClick.AddListener(() => Pubsub.Publisher.Scope<GameplayScope>().Publish(new OnEnterLevel(data)));
        DisplayTime().Forget();
    }

    public async UniTask DisplayTime()
    {
        var manager = SingleBehaviour.Of<LevelManager>();
        while (manager.GetLevelTime() >= 0)
        {
            _timeDisplay.text = "0:" + manager.GetLevelTime().ToString();
            await UniTask.Yield();
        }
    }

    public override UniTask Cleanup(Memory<object> args)
    {
        _resetButton.onClick.RemoveAllListeners();
        _homeButton.onClick.RemoveAllListeners();
        return base.Cleanup(args);
    }
}
