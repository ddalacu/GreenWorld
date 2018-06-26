using GreenProject.Controllers;
using UnityEngine;

public abstract class ControllerBase : MonoBehaviour
{
    public int ControllerIdentifier;

    public abstract void DoRequest(ControllerManager manager, GreenWorld world,
        GreenWorld.AdapterListener adapterListener,
        ControllerInputData inputData);
}

public abstract class ControllerGenericBase<T> : ControllerBase where T: ControllerInputData
{

    public override void DoRequest(ControllerManager manager, GreenWorld world, GreenWorld.AdapterListener adapterListener,
        ControllerInputData inputData)
    {
        OnDoRequest(manager, world, adapterListener, inputData as T);
    }

    protected abstract void OnDoRequest(ControllerManager manager, GreenWorld world, GreenWorld.AdapterListener adapterListener, T inputData);

}
