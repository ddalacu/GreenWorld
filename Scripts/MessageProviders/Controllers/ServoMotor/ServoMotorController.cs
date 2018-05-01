using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServoMotorController : ControllerBase
{
    public enum RotationAxis
    {
        X,
        Y,
        Z
    }

    public RotationAxis rotationAxis = RotationAxis.Y;

    public WorldTime worldTime;

    public float rotationSpeed = 2;

    private Coroutine rotating;

    public override Type GetDataType()
    {
        return typeof(ServoMotorInputData);
    }

    protected override void OnDoRequest(ControllerManager manager, GreenWorld world, GreenWorld.AdapterListener adapterListener, object inputData)
    {
        if (rotating != null)
        {
            manager.SendControllerResult(world, adapterListener, this, ControllerResult.Busy);
            return;
        }

        ServoMotorInputData servoMotorInputData = inputData as ServoMotorInputData;

        //manager.SendControllerProgress(world, adapterListener, this, .5f);

        rotating = StartCoroutine(StartRotate(manager, world, adapterListener, servoMotorInputData));
    }

    private IEnumerator StartRotate(ControllerManager manager, GreenWorld world, GreenWorld.AdapterListener adapterListener, ServoMotorInputData servoMotorInputData)
    {

        Quaternion targetRotation = Quaternion.identity;
        float initialDif = 0;

        switch (rotationAxis)
        {
            case RotationAxis.X:
                targetRotation = Quaternion.Euler(new Vector3(servoMotorInputData.DesiredAngle, 0, 0));
                initialDif = Mathf.DeltaAngle(transform.rotation.eulerAngles.x, servoMotorInputData.DesiredAngle);
                break;
            case RotationAxis.Y:
                targetRotation = Quaternion.Euler(new Vector3(0, servoMotorInputData.DesiredAngle, 0));
                initialDif = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, servoMotorInputData.DesiredAngle);
                break;
            case RotationAxis.Z:
                targetRotation = Quaternion.Euler(new Vector3(0, 0, servoMotorInputData.DesiredAngle));
                initialDif = Mathf.DeltaAngle(transform.rotation.eulerAngles.z, servoMotorInputData.DesiredAngle);
                break;
            default:
                break;
        }

        DateTime lastSendTime = worldTime.CurrentTime;

        while (transform.localRotation != targetRotation)
        {
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, rotationSpeed * Time.unscaledDeltaTime * worldTime.timeScale);

            float dif = 0;

            switch (rotationAxis)
            {
                case RotationAxis.X:
                    dif = Mathf.DeltaAngle(transform.rotation.eulerAngles.x, servoMotorInputData.DesiredAngle);
                    break;
                case RotationAxis.Y:
                    dif = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, servoMotorInputData.DesiredAngle);
                    break;
                case RotationAxis.Z:
                    dif = Mathf.DeltaAngle(transform.rotation.eulerAngles.z, servoMotorInputData.DesiredAngle);
                    break;
                default:
                    break;
            }

            float progress = 1 - (dif / initialDif);

            if ((worldTime.CurrentTime - lastSendTime).TotalSeconds > 0.25f)//send once 0.25 the progress
            {
                manager.SendControllerProgress(world, adapterListener, this, progress);
                lastSendTime = worldTime.CurrentTime;
            }

            yield return null;
        }

        manager.SendControllerResult(world, adapterListener, this, ControllerResult.Completed);
        rotating = null;
    }

}
