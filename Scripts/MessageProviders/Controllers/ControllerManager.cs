using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GreenProject.Controllers;
using GreenProject.Messages;
using UnityEngine;

public class ControllerManager : MonoBehaviour
{
    public ControllerBase[] ControllersGeneric = new ControllerBase[0];
    public GreenWorld GreenWorld;

    private void Start()
    {
        HashSet<ControllerBase> duplicateCheck = new HashSet<ControllerBase>();

        int length = ControllersGeneric.Length;

        for (int i = 0; i < length; i++)
        {
            if (duplicateCheck.Add(ControllersGeneric[i]) == false)
            {
                Debug.Log("Duplicate controllerGeneric ids");
                break;
            }
        }

        GreenWorld.AddMessageListener<ControllerInputData>(Listener);
    }

    private void Listener(GreenWorld.AdapterListener adapter, ControllerInputData inputData)
    {

        Debug.Log("Got controllerGeneric message !");

        int length = ControllersGeneric.Length;

        for (int i = 0; i < length; i++)
        {
            if (ControllersGeneric[i].ControllerIdentifier == inputData.ControllerIdentifier)
            {
                ControllersGeneric[i].DoRequest(this, GreenWorld, adapter, inputData);
                return;
            }
        }

        Debug.LogError($"Request to inexistent controllerGeneric { inputData.ControllerIdentifier}!");
    }

    [System.Serializable]
    public class ControllerOutputData
    {
        public int ControllerIdentifier;

        [SerializeField]
        protected int OutputType;
    }

    //tells the result of a controllerGeneric request
    public class ControllerResultInfo : ControllerOutputData
    {
        public ControllerResult Result;

        public ControllerResultInfo()
        {
            OutputType = 1;
        }
    }

    //tells the progress of a controllerGeneric request
    public class ControllerProgressInfo : ControllerOutputData
    {
        public float Progress;

        public ControllerProgressInfo()
        {
            OutputType = 2;
        }
    }

    public void SendControllerResult<TData>(GreenWorld world, GreenWorld.AdapterListener adapterListener, ControllerGenericBase<TData> controllerGeneric, ControllerResult controllerResult) where TData : ControllerInputData
    {
        world.SendMessage(adapterListener, new ControllerResultMessage(controllerResult, controllerGeneric.ControllerIdentifier));
    }

    public void SendControllerProgress<TData>(GreenWorld world, GreenWorld.AdapterListener adapterListener, ControllerGenericBase<TData> controllerGeneric, float progress) where TData : ControllerInputData
    {
        world.SendMessage(adapterListener, new ControllerProgressMessage(progress, controllerGeneric.ControllerIdentifier));
    }
}
