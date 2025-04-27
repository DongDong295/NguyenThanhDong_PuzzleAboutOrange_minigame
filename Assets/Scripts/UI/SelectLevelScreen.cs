using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using ZBase.Foundation.Pooling;
using ZBase.Foundation.Singletons;

public class SelectLevelScreen : ZBase.UnityScreenNavigator.Core.Screens.Screen
{
    [SerializeField] private GameObject _scrollContents;
    [SerializeField] private Button _backButton;
    private List<GameObject> _levelButtons;
    public override async UniTask Initialize(Memory<object> args)
    {
        _levelButtons = new List<GameObject>();
        _backButton.onClick.AddListener(() =>
            Pubsub.Publisher.Scope<UIScope>().Publish(new CloseScreenEvent()
        ));
        await LoadLevelButton();
    }

    private async UniTask LoadLevelButton()
    {
        var dataManager = Singleton.Of<DataManager>();
        Debug.Log(dataManager);
        LevelData levelData = await dataManager.Load<LevelData>("level-config-data");
        int index = 0;
        var playerLevelData = Singleton.Of<DataManager>().PlayerLevelData;
        foreach (var level in levelData.LevelDataItems)
        {
            index++;
            if (index <= playerLevelData.Count)
            {
                var levelButton = await LoadButton();
                levelButton.GetComponent<LevelButton>().InitializeButton(index, true, playerLevelData[index]);
                levelButton.transform.SetParent(_scrollContents.transform);
                var button = levelButton.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    Pubsub.Publisher.Scope<UIScope>().Publish(new ShowScreenEvent(Constants.UI.GAMEPLAY_SCREEN, false));
                    Pubsub.Publisher.Scope<GameplayScope>().Publish(new OnEnterLevel(level));
                }
                );
                _levelButtons.Add(levelButton);
            }
            else
            {
                var levelButton = await LoadButton();
                levelButton.GetComponent<LevelButton>().InitializeButton(index, false, 0);
                levelButton.transform.SetParent(_scrollContents.transform);
                _levelButtons.Add(levelButton);
            }
        }
    }

    private async UniTask<GameObject> LoadButton()
    {
        var btn = await SingleBehaviour.Of<PoolingManager>().Rent("level-button-prefab");
        return btn;
    }

    public override UniTask Cleanup(Memory<object> args)
    {
        _backButton.onClick.RemoveAllListeners();
        foreach (var b in _levelButtons)
        {
            b.GetComponent<Button>().onClick.RemoveAllListeners();
            SingleBehaviour.Of<PoolingManager>().Return(b);
        }
        return base.Cleanup(args);
    }
}
