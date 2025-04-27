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

public class FinishLevelWinModal : Modal
{
    [SerializeField] private GameObject _starsHolder;
    [SerializeField] private Button _replayButton;
    [SerializeField] private Button _homeButton;
    [SerializeField] private Button _nextLevelButton;
    [SerializeField] private Sprite _goldStarSprite;
    [SerializeField] private Sprite _blackStarSprite;

    private int _stars;
    private bool _isWin;
    public override async UniTask Initialize(Memory<object> args)
    {
        _stars = (int)args.Span[0];
        _isWin = (bool)args.Span[1];
        var levelManager = SingleBehaviour.Of<LevelManager>();
        var dataManager = Singleton.Of<DataManager>();
        var levelData = await dataManager.Load<LevelData>("level-config-data");
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
                new OnEnterLevel(SingleBehaviour.Of<LevelManager>().GetCurrentLevelData())
            );
            Pubsub.Publisher.Scope<UIScope>().Publish(
                new CloseModalEvent());
        });
        _nextLevelButton.onClick.AddListener(() =>
        {
            if (levelManager.GetCurrentLevelIndex() < levelData.LevelDataItems.Count())
            {
                var index = levelManager.GetCurrentLevelIndex();
                Pubsub.Publisher.Scope<GameplayScope>().Publish(new OnEnterLevel(levelData.LevelDataItems[index++]));
                Pubsub.Publisher.Scope<UIScope>().Publish(
                new CloseModalEvent());
            }
        });
        await UniTask.CompletedTask;
    }

    public override void DidPushEnter(Memory<object> args)
    {
        UpdateVisual().Forget();
    }

    private async UniTask UpdateVisual()
    {
        var starIcons = _starsHolder.GetComponentsInChildren<Image>().ToList();
        for (int i = 0; i < starIcons.Count; i++)
        {
            if (i < _stars)
            {
                starIcons[i].sprite = _goldStarSprite;
            }
            else
            {
                starIcons[i].sprite = _blackStarSprite;
            }
            var sequence = DOTween.Sequence();
            starIcons[i].transform.localScale = Vector3.one * 3;
            sequence.Append(starIcons[i].transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
            await UniTask.WaitForSeconds(0.2f);
        }
    }

    public override UniTask Cleanup(Memory<object> args)
    {
        _nextLevelButton.onClick.RemoveAllListeners();
        _homeButton.onClick.RemoveAllListeners();
        _replayButton.onClick.RemoveAllListeners();
        return base.Cleanup(args);
    }
}
