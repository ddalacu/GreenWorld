using GreenProject.Messages;
using JetBrains.Annotations;
using UnityEngine;

public class LightProvider : MonoBehaviour
{
    public GreenWorld GreenWorld;

    [UsedImplicitly]
    private void Awake()
    {
        GreenWorld.AddMessageListener(LightRequestListener, typeof(LightRequestMessage));
    }

    private void LightRequestListener(GreenWorld.AdapterListener adapter, INetworkMessage networkMessage)
    {
        Debug.Log("Light status requested!");

        if(Random.Range(0,2)==0)
        {
            GreenWorld.SendMessage(adapter, new LightResponseMessage(LightResponseMessage.LightStatus.Light));
        }
        else
        {
            GreenWorld.SendMessage(adapter, new LightResponseMessage(LightResponseMessage.LightStatus.Dark));
        }
    }

}
