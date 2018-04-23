using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TemperatureProvider : WorldMessageProvider
{
    public override int GetTypeIdentifier()
    {
        return 112;
    }

    public Light sun;

    public AnimationCurve temperatureCurve;

    public float temperatureMultiplier;

    [Range(0, 1)]
    public float temperatureVariation = 0.2f;

    private float temperature = 0;
    private float lastVariation = 0;

    public float GetTemperatureFromColorWarmth(Color color)
    {
        float warmth = color.r + color.g + color.b;
        return warmth / 3;
    }

    private void Update()
    {
        float variation = UnityEngine.Random.Range(-temperatureVariation / 2, temperatureVariation / 2) * temperatureMultiplier;
        float cTemp = temperatureCurve.Evaluate(GetTemperatureFromColorWarmth(sun.color)) * temperatureMultiplier;
        cTemp += variation;

        temperature = Mathf.Lerp(temperature, cTemp + lastVariation, Time.deltaTime);
        lastVariation = Mathf.Lerp(lastVariation, variation, 0.5f);
    }

    public override void HandleMessage(GreenWorld world, GreenWorld.AdapterListener adapterListener, byte[] data)
    {
        Debug.Log(Encoding.ASCII.GetString(data));
        world.SendWorldMessage(adapterListener, BitConverter.GetBytes(temperature), GetTypeIdentifier());
    }
}
