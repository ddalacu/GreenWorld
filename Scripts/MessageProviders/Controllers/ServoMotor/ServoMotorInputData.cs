using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServoMotorInputData
{
    [SerializeField]
    private float _desiredAngle;

    public float DesiredAngle
    {
        get
        {
            return _desiredAngle;
        }
    }
}
