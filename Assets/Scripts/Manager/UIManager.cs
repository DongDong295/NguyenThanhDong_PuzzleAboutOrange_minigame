using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;
using ZBase.Foundation.PubSub;
using ZBase.Foundation.Singletons;
using ZBase.UnityScreenNavigator.Core.Modals;
using ZBase.UnityScreenNavigator.Core.Screens;
using ZBase.UnityScreenNavigator.Core.Views;

public class UIManager : MonoBehaviour, IManager
{
    private List<ISubscription> _subscription;
    private Queue<ShowScreenEvent> _screenQueue = new Queue<ShowScreenEvent>();
    private bool _isShowingScreen = false;
    public async UniTask OnApplicationStart()
    {
        Debug.Log("CC");
        _subscription = new List<ISubscription>();
        _subscription.Add(Pubsub.Subscriber.Scope<UIScope>().Subscribe<ShowScreenEvent>(OnShowScreen));
        _subscription.Add(Pubsub.Subscriber.Scope<UIScope>().Subscribe<ShowModalEvent>(OnShowModal));
        _subscription.Add(Pubsub.Subscriber.Scope<UIScope>().Subscribe<CloseModalEvent>(
            async (e) => await OnCloseModal(e, false)));
        _subscription.Add(Pubsub.Subscriber.Scope<UIScope>().Subscribe<CloseScreenEvent>(
           async (e) => await OnCloseScreen(e, false)));
        await UniTask.WaitUntil(() => ScreenLauncher.ContainerManager != null);
        await UniTask.CompletedTask;
    }


    public void OnShowScreen(ShowScreenEvent e)
    {
        _screenQueue.Enqueue(e);
        if (!_isShowingScreen)
        {
            ShowNextScreen().Forget();
        }
    }


    private async UniTaskVoid ShowNextScreen()
    {
        if (_screenQueue.Count == 0)
        {
            _isShowingScreen = false;
            return;
        }

        _isShowingScreen = true;
        var e = _screenQueue.Dequeue();
        var options = new ViewOptions(e.Path, true, loadAsync: e.LoadAsync);
        await ScreenLauncher.ContainerManager.Find<ScreenContainer>().PushAsync(options);
        await UniTask.DelayFrame(1);
        ShowNextScreen().Forget();
    }

    public async UniTask OnCloseScreen(CloseScreenEvent e, bool isPlayAnim)
    {
        await ScreenLauncher.ContainerManager.Find<ScreenContainer>().PopAsync(isPlayAnim);
    }

    public async void OnShowModal(ShowModalEvent e)
    {
        var options = new ModalOptions(e.Path, true, loadAsync: e.LoadAsync);
        await ScreenLauncher.ContainerManager.Find<ModalContainer>().PushAsync(options);
    }

    public async UniTask OnCloseModal(CloseModalEvent e, bool isPlayAnim)
    {
        await ScreenLauncher.ContainerManager.Find<ModalContainer>().PopAsync(isPlayAnim);
    }

    public Modal GetCurrentModal()
    {
        var modalContainer = ScreenLauncher.ContainerManager.Find<ModalContainer>();
        if (modalContainer != null)
        {
            return modalContainer.Current.View;
        }
        return null;
    }
}