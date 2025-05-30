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
        _playButton.gameObject.SetActive(true);
        _playButton.onClick.AddListener(() => OnPlayButtonClicked());
        return base.Initialize(args);
    }

    public override void DidPushEnter(Memory<object> args)
    {
        AnimateUI().Forget();
    }

    public override void DidPopEnter(Memory<object> args)
    {
        _playButton.gameObject.SetActive(true);
        base.DidPopEnter(args);
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

    private void OnPlayButtonClicked()
    {
        _playButton.transform.DOScale(Vector3.one * 0.35f, 0.35f).SetEase(Ease.InBack).OnComplete(() =>
        {
            _playButton.gameObject.SetActive(false);
            GoToSelectLevelScreen();
            _playButton.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        });
    }

    public override UniTask Cleanup(Memory<object> args)
    {
        _playButton.onClick.RemoveAllListeners();
        return base.Cleanup(args);
    }
}
