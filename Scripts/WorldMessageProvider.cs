using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WorldMessageProvider : MonoBehaviour
{
    public abstract int GetTypeIdentifier();

    public abstract void HandleMessage(GreenWorld world, GreenWorld.AdapterListener adapterListener, byte[] data);
}
