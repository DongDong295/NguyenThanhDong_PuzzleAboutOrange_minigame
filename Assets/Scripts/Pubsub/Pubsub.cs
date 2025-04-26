using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using ZBase.Foundation.PubSub;

public class Pubsub
{
    private static Messenger s_instance;
    private static Messenger Instance => s_instance ??= new();
    public static MessagePublisher Publisher = Instance.MessagePublisher;
    public static MessageSubscriber Subscriber => Instance.MessageSubscriber;
}