using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTime : MonoBehaviour
{

    public DateTime CurrentTime { get; set; }

    //from 0 to 1

    public float timeScale = 1;
    private float time = 0;

    private void Update()
    {
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
