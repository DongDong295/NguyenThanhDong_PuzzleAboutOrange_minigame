using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class MainMenuScreen : ZBase.UnityScreenNavigator.Core.Screens.Screen
{
    [SerializeField] private GameObject _gameTitle;
    [SerializeField] private Button _playButton;

    public override UniTask Initialize(Memory<object> args)
    {
        _playButton.onClick.AddListener(() => GoToSelectLevelScreen());
        return base.Initialize(args);
    }
    public override void DidPushEnter(Memory<object> args)
    {
        AnimateUI().Forget();
    }

    private void GoToSelectLevelScreen()
    {
        Pubsub.Publisher.Scope<UIScope>().Publish(new ShowScreenEvent(Constants.UI.SELECT_LEVEL_SCREEN, false));
    }

    private async UniTaskVoid AnimateUI()
    {
        _gameTitle.transform.localScale = Vector3.zero;
        _playButton.transform.localScale = Vector3.zero;
        await _gameTitle.transform
            .DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack)
            .AsyncWaitForCompletion();

        await _playButton.transform
            .DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack)
            .AsyncWaitForCompletion();
    }

    public override UniTask Cleanup(Memory<object> args)
    {
        _playButton.onClick.RemoveAllListeners();
        return base.Cleanup(args);
    }
}
