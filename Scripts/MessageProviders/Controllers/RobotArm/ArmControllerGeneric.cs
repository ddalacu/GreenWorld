using System;
using System.Collections;
using System.Collections.Generic;
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
        ArmControllerInputData armControllerInputData = inputData as ArmControllerInputData;

        Debug.Log("Reching point " + armControllerInputData);

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
        }, new Vector3(armControllerInputData.X, armControllerInputData.Y, armControllerInputData.Z));
    }
}
