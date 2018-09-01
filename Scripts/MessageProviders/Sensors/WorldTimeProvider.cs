using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GreenProject.Messages;
using JetBrains.Annotations;
using UnityEngine;

public class WorldTimeProvider : MonoBehaviour
{
    public WorldTime WorldTime;
    public GreenWorld GreenWorld;

    [UsedImplicitly]
    private void Awake()
    {
        GreenWorld.AddMessageListener(RequestedTime,typeof(TimeRequestMessage));
    }

    private void RequestedTime(GreenWorld.AdapterListener adapterListener, INetworkMessage message)
    {
        Debug.Log("Time was requested!");
        GreenWorld.SendMessage(adapterListener, new TimeResponseMessage(WorldTime.CurrentTime));
    }
}
