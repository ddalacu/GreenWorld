using System;
using GreenProject.Controllers;

public abstract class ControllerGenericBase<T> : ControllerBase where T: ControllerInputData
{
    public override Type GetDataType()
    {
        return typeof(T);
    }

    public override void DoRequest(ControllerManager manager, GreenWorld world, GreenWorld.AdapterListener adapterListener,
        ControllerInputData inputData)
    {
        OnDoRequest(manager, world, adapterListener, inputData as T);
    }

    protected abstract void OnDoRequest(ControllerManager manager, GreenWorld world, GreenWorld.AdapterListener adapterListener, T inputData);

}
