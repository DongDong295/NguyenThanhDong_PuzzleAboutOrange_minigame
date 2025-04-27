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
        _resetButton.onClick.AddListener(() => Pubsub.Publisher.Scope<GameplayScope>().Publish(new OnEnterLevel(_manager.GetCurrentLevelData())));
        DisplayTime().Forget();
    }

    public async UniTask DisplayTime()
    {
        var manager = SingleBehaviour.Of<LevelManager>();
        bool isLastTenSeconds = false;

        while (manager.GetLevelTime() >= 0)
        {
            int timeLeft = (int)manager.GetLevelTime();
            _timeDisplay.text = "0:" + timeLeft.ToString();

            if (timeLeft <= 10)
            {
                if (!isLastTenSeconds)
                {
                    isLastTenSeconds = true;
                }
                AnimateTimer();
            }

            await UniTask.Delay(1000);
        }
    }

    private void AnimateTimer()
    {
        _timeDisplay.transform.DOKill();
        _timeDisplay.transform.localScale = Vector3.one * 1.5f;
        _timeDisplay.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
    }

    public override UniTask Cleanup(Memory<object> args)
    {
        _resetButton.onClick.RemoveAllListeners();
        _homeButton.onClick.RemoveAllListeners();
        return base.Cleanup(args);
    }
}
