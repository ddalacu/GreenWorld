using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TemperatureProvider : WorldMessageProvider
{
    public override int GetTypeIdentifier()
    {
        return 112;
    }

    public override void HandleMessage(GreenWorld world, GreenWorld.AdapterListener adapterListener, byte[] data)
    {
        Debug.Log(Encoding.ASCII.GetString(data));
    }
}
