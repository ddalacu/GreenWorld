using GreenProject.Controllers;
using UnityEngine;

public class ArmControllerGeneric : ControllerGenericBase<ArmControllerInputData>
{
    public JointAssembly JointAssembly;

    protected override void OnDoRequest(ControllerManager manager, GreenWorld world, GreenWorld.AdapterListener adapterListener, ArmControllerInputData inputData)
    {
        if (JointAssembly.IsBuisy)
        {
            manager.SendControllerResult(world, adapterListener, this, ControllerResult.Busy);
            return;
        }

        if (inputData == null)
        {
            Debug.LogError("Null armControllerInputData!");
            return;
        }

        Debug.Log("Reching point " + inputData);

        JointAssembly.ReachPoint((result) =>
        {
            if (result)
            {
                manager.SendControllerResult(world, adapterListener, this, ControllerResult.Completed);
            }
            else
            {
                manager.SendControllerResult(world, adapterListener, this, ControllerResult.Fail);
            }
        }, new Vector3(inputData.X, inputData.Y, inputData.Z));
    }
}
