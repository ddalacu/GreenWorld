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
        GreenWorld.AddMessageListener<TimeRequestMessage>(RequestedTime);
    }

    private void RequestedTime(GreenWorld.AdapterListener adapterListener, TimeRequestMessage networkmessage)
    {
        Debug.Log("Time was requested!");
        GreenWorld.SendMessage(adapterListener, new TimeResponseMessage(WorldTime.CurrentTime));
    }
}
