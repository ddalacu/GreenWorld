using System.Collections;
using GreenProject.Controllers;
using UnityEngine;

public class PlanterControllerGeneric : ControllerGenericBase<PlanterControllerInputData>
{
    private Coroutine _plantingRoutine;

    protected override void OnDoRequest(ControllerManager manager, GreenWorld world, GreenWorld.AdapterListener adapterListener, PlanterControllerInputData inputData)
    {
        if (_plantingRoutine != null)
        {
            manager.SendControllerResult(world, adapterListener, this, ControllerResult.Busy);
            return;
        }

        if (inputData == null)
        {
            Debug.LogError("Null armControllerInputData!");
            return;
        }

        Debug.Log("Planting at index " + inputData.PlantIndex);
        _plantingRoutine = StartCoroutine(Plant(manager, world, adapterListener));
    }

    private IEnumerator Plant(ControllerManager manager, GreenWorld world, GreenWorld.AdapterListener adapterListener)
    {
        yield return new WaitForSeconds(10);
        _plantingRoutine = null;
        manager.SendControllerResult(world, adapterListener, this, ControllerResult.Completed);
    }

}
