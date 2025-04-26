using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZBase.Foundation.Pooling;
using ZBase.Foundation.PubSub;

public struct UIScope { }

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
public struct ShowModalEvent : IMessage
{
    public string Path;
    public bool LoadAsync;
    public ShowModalEvent(string path, bool loadAsync)
    {
        Path = path;
        LoadAsync = loadAsync;
    }
}
public struct CloseModalEvent : IMessage { }