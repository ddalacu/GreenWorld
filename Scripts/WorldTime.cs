using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTime : MonoBehaviour
{
    public Light sunLight;

    public DateTime CurrentTime { get; set; }

    //from 0 to 1

    public float timeScale = 1;
    private float time = 0;

    public Gradient nightDayColor;

    public float maxIntensity = 3f;
    public float minIntensity = 0f;
    public float minPoint = -0.2f;

    public float maxAmbient = 1f;
    public float minAmbient = 0f;
    public float minAmbientPoint = -0.2f;

    public Vector3 rotateSpeed;
    public Vector3 initialSunRotation;
    public Vector3 offsetRotation;

    private void Awake()
    {
        sunLight.transform.localRotation = Quaternion.Euler(initialSunRotation);
    }

    private void Update()
    {

        float tRange = 1 - minPoint;
        float dot = Mathf.Clamp01((Vector3.Dot(sunLight.transform.forward, Vector3.down) - minPoint) / tRange);
        float i = ((maxIntensity - minIntensity) * dot) + minIntensity;

        sunLight.intensity = i;

        tRange = 1 - minAmbientPoint;
        dot = Mathf.Clamp01((Vector3.Dot(sunLight.transform.forward, Vector3.down) - minAmbientPoint) / tRange);
        i = ((maxAmbient - minAmbient) * dot) + minAmbient;
        RenderSettings.ambientIntensity = i;

        sunLight.color = nightDayColor.Evaluate(dot);
        RenderSettings.ambientLight = sunLight.color;

        float daySeconds = 86400;

        int hours = 0, minutes = 0, seconds = 0, totalSeconds = 0;
        hours = (24 - CurrentTime.Hour) - 1;
        minutes = (60 - CurrentTime.Minute) - 1;
        seconds = (60 - CurrentTime.Second - 1);
        totalSeconds = seconds + (minutes * 60) + (hours * 3600);

        float progress = (CurrentTime.Second + totalSeconds) / daySeconds;
        sunLight.transform.rotation = Quaternion.Euler( offsetRotation+ new Vector3(rotateSpeed.x, rotateSpeed.y, rotateSpeed.z) * (progress * 360));

        time += Time.unscaledDeltaTime * timeScale;
    }

    private void OnGUI()
    {
        TimeSpan time = TimeSpan.FromSeconds(this.time);
        CurrentTime = DateTime.Today.Add(time);

        GUILayout.Label("Seconds since startup " + Mathf.Round(Time.unscaledTime));

        GUILayout.Label("Seconds since startup(scaled) " + Mathf.Round(this.time));
        GUILayout.Label(CurrentTime.ToString("hh:mm:tt"));
    }

}
