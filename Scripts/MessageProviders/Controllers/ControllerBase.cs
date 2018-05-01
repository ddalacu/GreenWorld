using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public abstract class ControllerBase : MonoBehaviour
{
    public int controllerIdentifier;

    public abstract Type GetDataType();

    public void DoAction(ControllerManager manager, GreenWorld world, GreenWorld.AdapterListener adapterListener, object obj)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        if (obj.GetType() != GetDataType())
        {
            throw new ArgumentException(nameof(obj));
        }

        OnDoRequest(manager, world, adapterListener, obj);
    }

    protected abstract void OnDoRequest(ControllerManager manager, GreenWorld world, GreenWorld.AdapterListener adapterListener, object inputData);

}
