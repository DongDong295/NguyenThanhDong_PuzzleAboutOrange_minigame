using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using ZBase.Foundation.Singletons;
using ZBase.UnityScreenNavigator.Core.Modals;

public class FinishLevelLoseModal : Modal
{
    [SerializeField] private GameObject _starsHolder;
    [SerializeField] private Button _replayButton;
    [SerializeField] private Button _homeButton;
    [SerializeField] private Button _nextLevelButton;
    [SerializeField] private Sprite _goldStarSprite;
    [SerializeField] private Sprite _blackStarSprite;

    private int _stars;

    private List<Image> _starIcons;
    public override async UniTask Initialize(Memory<object> args)
    {
        _stars = 0;
        _starIcons = new List<Image>();
        _starIcons = _starsHolder.GetComponentsInChildren<Image>().ToList();

        foreach (var s in _starIcons)
        {
            s.transform.localScale = Vector3.zero;
        }

        _homeButton.onClick.AddListener(() =>
        {
            Pubsub.Publisher.Scope<UIScope>().Publish(
                new ShowScreenEvent(Constants.UI.MAIN_MENU_SCREEN, false));
            Pubsub.Publisher.Scope<UIScope>().Publish(
                new CloseModalEvent());
        });
        _replayButton.onClick.AddListener(() =>
        {
            Pubsub.Publisher.Scope<GameplayScope>().Publish(
                new OnEnterLevel(SingleBehaviour.Of<LevelManager>().GetCurrentLevelData()));
            Pubsub.Publisher.Scope<UIScope>().Publish(
                new CloseModalEvent());
        });
        await UniTask.CompletedTask;
    }

    public override void DidPushEnter(Memory<object> args)
    {
        UpdateVisual().Forget();
    }

    private async UniTask UpdateVisual()
    {
        for (int i = 0; i < _starIcons.Count; i++)
        {
            if (i < _stars)
            {
                _starIcons[i].sprite = _goldStarSprite;
            }
            else
            {
                _starIcons[i].sprite = _blackStarSprite;
            }
            var sequence = DOTween.Sequence();
            _starIcons[i].transform.localScale = Vector3.one * 3;
            sequence.Append(_starIcons[i].transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
            await UniTask.WaitForSeconds(0.2f);
        }

    }
    public override UniTask Cleanup(Memory<object> args)
    {
        _homeButton.onClick.RemoveAllListeners();
        _replayButton.onClick.RemoveAllListeners();
        return base.Cleanup(args);
    }
}
