using GreenProject.Messages;
using JetBrains.Annotations;
using UnityEngine;

public class LightProvider : MonoBehaviour
{
    public GreenWorld GreenWorld;

    public Light Light;

    private bool _isLight;

    [UsedImplicitly]
    private void Awake()
    {
        GreenWorld.AddMessageListener(LightRequestListener, typeof(LightRequestMessage));
        _isLight = false;
    }

    private void Update()
    {
        if (Light != null)
            Light.enabled = _isLight;
    }

    private void OnGUI()
    {
        GUILayout.Space(200);
        _isLight = GUILayout.Toggle(_isLight, "IsDay");
    }


    private void LightRequestListener(GreenWorld.AdapterListener adapter, INetworkMessage networkMessage)
    {
        Debug.Log("Light status requested!");
        GreenWorld.SendMessage(adapter, new LightResponseMessage(_isLight ? LightResponseMessage.LightStatus.Light : LightResponseMessage.LightStatus.Dark));
    }

}
