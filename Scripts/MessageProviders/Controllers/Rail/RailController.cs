using System;
using System.Collections;
using GreenProject.Controllers;
using UnityEngine;

public class RailController : ControllerGenericBase<RailInputData>
{
    public float RailLength;
    public float RailEndSize;

    public Transform[] PlantsMagnets;

    public Transform ArmTransform;

    private Coroutine _activeCoroutine;

    protected override void OnDoRequest(ControllerManager manager, GreenWorld world, GreenWorld.AdapterListener adapterListener, RailInputData inputData)
    {
        if (_activeCoroutine != null)
        {
            manager.SendControllerResult(world, adapterListener, this, ControllerResult.Busy);
            return;
        }

        switch (inputData.ToExecute)
        {
            case RailInputData.Action.First:
                _activeCoroutine = StartCoroutine(FirstMagnet(b =>
                  {
                      manager.SendControllerResult(world, adapterListener, this, b ? ControllerResult.Completed : ControllerResult.Fail);
                  }));
                break;
            case RailInputData.Action.Next:
                _activeCoroutine = StartCoroutine(NextMagnet(b =>
                  {
                      manager.SendControllerResult(world, adapterListener, this, b ? ControllerResult.Completed : ControllerResult.Fail);
                  }));
                break;
        }
    }

    private IEnumerator Reset()
    {
        Debug.Log("Reseting rail!");

        while (IsTouchingStart() == false)
        {
            ArmTransform.position += Vector3.back * Time.deltaTime;
            yield return null;
        }

        Debug.Log("Reseted rail!");
    }

    private Transform GetClosestMagnetInRange(float minDist = 0.25f)
    {
        Transform closest = null;

        for (int i = 0; i < PlantsMagnets.Length; i++)
        {
            Vector3 pos = PlantsMagnets[i].position;

            float dist = Mathf.Abs(pos.z - ArmTransform.position.z);
            if (dist < minDist)
            {
                minDist = dist;
                closest = PlantsMagnets[i];
            }
        }

        return closest;
    }

    private IEnumerator NextMagnet(Action<bool> reached)
    {
        Transform currentMagnet = GetClosestMagnetInRange();

        while (GetClosestMagnetInRange() == currentMagnet || GetClosestMagnetInRange() == null)
        {
            ArmTransform.position += Vector3.forward * Time.deltaTime;
            if (IsTouchingEnd())
            {
                reached(false);
                yield break;
            }

            yield return null;
        }

        reached(true);
        _activeCoroutine = null;
    }

    private IEnumerator FirstMagnet(Action<bool> reached)
    {
        yield return StartCoroutine(Reset());
        yield return StartCoroutine(NextMagnet(reached));
    }

    private bool IsTouchingEnd()
    {
        Vector3 max = transform.position + transform.forward * ((RailLength / 2));
        return max.z <= ArmTransform.position.z;
    }

    private bool IsTouchingStart()
    {
        Vector3 min = transform.position - transform.forward * ((RailLength / 2));
        return min.z >= ArmTransform.position.z;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0,1,1,0.5f);

        if (IsTouchingEnd())
            Gizmos.color = new Color(1, 0, 0, 0.5f);

        if (IsTouchingStart())
            Gizmos.color = new Color(0, 1, 0, 0.5f);

        Gizmos.DrawCube(transform.position + transform.forward * ((RailLength / 2) + (RailEndSize / 2)), new Vector3(20, 20, RailEndSize));
        Gizmos.DrawCube(transform.position - transform.forward * ((RailLength / 2) + (RailEndSize / 2)), new Vector3(20, 20, RailEndSize));

    }

}
