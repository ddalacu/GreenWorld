using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ControllerManager : WorldMessageProvider
{
    public ControllerBase[] controllers = new ControllerBase[0];

    private void Start()
    {
        HashSet<ControllerBase> duplicateCheck = new HashSet<ControllerBase>();

        int length = controllers.Length;

        for (int i = 0; i < length; i++)
        {
            if (duplicateCheck.Add(controllers[i]) == false)
            {
                Debug.Log("Duplicate controller ids");
                break;
            }
        }
    }

    public override int GetTypeIdentifier()
    {
        return 4;
    }

    public override void HandleMessage(GreenWorld world, GreenWorld.AdapterListener adapterListener, byte[] data)
    {
        string json = Encoding.ASCII.GetString(data);
        ControllerInputData inputId = JsonUtility.FromJson<ControllerInputData>(json);

        int length = controllers.Length;

        for (int i = 0; i < length; i++)
        {
            if (controllers[i].controllerIdentifier == inputId.controllerIdentifier)
            {
                controllers[i].DoAction(this, world, adapterListener, JsonUtility.FromJson(json, controllers[i].GetDataType()));
                return;
            }
        }

        Debug.LogError($"Request to inexistent controller { inputId.controllerIdentifier}!");
    }

    public class ControllerOutputData
    {
        public int controllerIdentifier;

        [SerializeField]
        protected int outputType;
    }

    public class ControllerResultInfo : ControllerOutputData
    {
        public ControllerResult result;

        public ControllerResultInfo()
        {
            outputType = 1;
        }
    }

    public class ControllerProgressInfo : ControllerOutputData
    {
        public float progress;

        public ControllerProgressInfo()
        {
            outputType = 2;
        }
    }

    public void SendControllerResult(GreenWorld world, GreenWorld.AdapterListener adapterListener, ControllerBase controller, ControllerResult controllerResult)
    {
        ControllerResultInfo controllerResultInfo = new ControllerResultInfo()
        {
            controllerIdentifier = controller.controllerIdentifier,
            result = controllerResult
        };

        Send(world, adapterListener, controllerResultInfo);
    }

    public void SendControllerProgress(GreenWorld world, GreenWorld.AdapterListener adapterListener, ControllerBase controller, float progress)
    {
        ControllerProgressInfo controllerProgressInfo = new ControllerProgressInfo()
        {
            controllerIdentifier = controller.controllerIdentifier,
            progress = progress
        };

        Send(world, adapterListener, controllerProgressInfo);
    }

    private void Send(GreenWorld world, GreenWorld.AdapterListener adapterListener, object obj)
    {
        string json = JsonUtility.ToJson(obj);
        world.SendWorldMessage(adapterListener, Encoding.ASCII.GetBytes(json), GetTypeIdentifier());
    }

}
