using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GreenProject.Sensors.Temperature;
using JetBrains.Annotations;
using UnityEngine;

public class TemperatureProvider : MonoBehaviour
{
    public GreenWorld GreenWorld;

    public Light sun;

    public AnimationCurve temperatureCurve;

    public float temperatureMultiplier;

    [Range(0, 1)]
    public float temperatureVariation = 0.2f;

    private float temperature = 0;
    private float lastVariation = 0;

    private void Awake()
    {
        GreenWorld.AddMessageListener<GetTemperatureMessage>(TemperatureRequestListener);
    }

    private void TemperatureRequestListener(GreenWorld.AdapterListener adapter, GetTemperatureMessage networkMessage)
    {
        Debug.Log("Temperature request!");
        GreenWorld.SendMessage(adapter, new TemperatureResponseMessage(temperature));
    }

    public float GetTemperatureFromColorWarmth(Color color)
    {
        float warmth = color.r + color.g + color.b;
        return warmth / 3;
    }

    [UsedImplicitly]
    private void Update()
    {
        float variation = UnityEngine.Random.Range(-temperatureVariation / 2, temperatureVariation / 2) * temperatureMultiplier;
        float cTemp = temperatureCurve.Evaluate(GetTemperatureFromColorWarmth(sun.color)) * temperatureMultiplier;
        cTemp += variation;

        temperature = Mathf.Lerp(temperature, cTemp + lastVariation, Time.deltaTime);
        lastVariation = Mathf.Lerp(lastVariation, variation, 0.5f);
    }
}
