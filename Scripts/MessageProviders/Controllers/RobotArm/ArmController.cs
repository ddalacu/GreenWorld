using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmController : ControllerBase
{
    public JointAssembly jointAssembly;

    public override Type GetDataType()
    {
        return typeof(ArmControllerInputData);
    }

    protected override void OnDoRequest(ControllerManager manager, GreenWorld world, GreenWorld.AdapterListener adapterListener, object inputData)
    {
        if (jointAssembly.IsBuisy)
        {
            manager.SendControllerResult(world, adapterListener, this, ControllerResult.Busy);
            return;
        }

        ArmControllerInputData armControllerInputData = inputData as ArmControllerInputData;

        jointAssembly.ReachPoint((result) =>
        {
            if (result)
            {
                manager.SendControllerResult(world, adapterListener, this, ControllerResult.Completed);
            }
            else
            {
                manager.SendControllerResult(world, adapterListener, this, ControllerResult.Fail);
            }
        }, new Vector3(armControllerInputData.x, armControllerInputData.y, armControllerInputData.z));
    }
}
