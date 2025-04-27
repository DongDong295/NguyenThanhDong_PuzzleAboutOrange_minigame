using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZBase.Foundation.Pooling;
using ZBase.Foundation.PubSub;

public struct UIScope { }
public struct GameplayScope { }

public struct OnEnterLevel : IMessage
{
    public LevelDataItem Data;
    public OnEnterLevel(LevelDataItem data)
    {
        Data = data;
    }
}

public struct OnFinishLevel : IMessage
{
    public int LevelIndex;
    public int Stars;

    public OnFinishLevel(int index, int stars)
    {
        LevelIndex = index;
        Stars = stars;
    }
}

public struct ShowScreenEvent : IMessage
{
    public string Path;
    public bool LoadAsync;
    public ShowScreenEvent(string path, bool loadAsync)
    {
        Path = path;
        LoadAsync = loadAsync;
    }
}

public struct CloseScreenEvent : IMessage { }
public class ShowModalEvent : IMessage
{
    public string Path;
    public bool LoadAsync;
    public Memory<object> Args;
    public ShowModalEvent(string path, bool loadAsync)
    {
        Path = path;
        LoadAsync = loadAsync;
    }
    public ShowModalEvent(string path, bool loadAsync, Memory<object> args)
    {
        Path = path;
        LoadAsync = loadAsync;
        Args = args;
    }
}
public struct CloseModalEvent : IMessage { }