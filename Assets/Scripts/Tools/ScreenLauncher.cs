using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZBase.UnityScreenNavigator.Core;
using ZBase.UnityScreenNavigator.Core.Screens;
using ZBase.UnityScreenNavigator.Core.Views;
using ZBase.UnityScreenNavigator.Core.Windows;

public class ScreenLauncher : UnityScreenNavigatorLauncher
{
    public static WindowContainerManager ContainerManager { get; private set; }

    protected override void OnAwake()
    {
        ContainerManager = this;
    }
    protected override void OnPostCreateContainers()
    {
    }
}