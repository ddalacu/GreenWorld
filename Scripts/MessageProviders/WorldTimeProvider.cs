using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class WorldTimeProvider : WorldMessageProvider
{
    public WorldTime worldTime;

    public override int GetTypeIdentifier()
    {
        return 2;
    }

    public override void HandleMessage(GreenWorld world, GreenWorld.AdapterListener adapterListener, byte[] data)
    {
        Debug.Log(Encoding.ASCII.GetString(data));
        world.SendWorldMessage(adapterListener, BitConverter.GetBytes(worldTime.CurrentTime.ToBinary()), GetTypeIdentifier());
    }
}
