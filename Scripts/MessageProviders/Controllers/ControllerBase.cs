using System;
using GreenProject.Controllers;
using UnityEngine;

public abstract class ControllerBase : MonoBehaviour
{
    public int ControllerIdentifier;

    public abstract Type GetDataType();

    public abstract void DoRequest(ControllerManager manager, GreenWorld world,
        GreenWorld.AdapterListener adapterListener,
        ControllerInputData inputData);
}