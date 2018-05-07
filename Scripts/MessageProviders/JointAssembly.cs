using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JointAssembly : MonoBehaviour
{
    [System.Serializable]
    public class Joint
    {
        public enum Axis
        {
            X,
            Y,
            Z
        }

        public Axis axis;
        public float angle;
        public float minAngle;
        public float maxAngle;
        public bool doIk = true;

        public Vector3 offset;

        public Transform transform;

        public static Vector3 GetAxisRotation(Axis axis, float rotation)
        {
            switch (axis)
            {
                case Axis.X:
                    return new Vector3(rotation, 0, 0);
                case Axis.Y:
                    return new Vector3(0, rotation, 0);
                case Axis.Z:
                    return new Vector3(0, 0, rotation);
                default:
                    break;
            }

            throw new System.NotImplementedException();
        }

        public static Vector3 GetReplacedAxisRotation(Axis axis, Vector3 euler, float rotation)
        {
            switch (axis)
            {
                case Axis.X:
                    return new Vector3(rotation, euler.y, euler.z);
                case Axis.Y:
                    return new Vector3(euler.x, rotation, euler.z);
                case Axis.Z:
                    return new Vector3(euler.x, euler.y, rotation);
                default:
                    break;
            }

            throw new System.NotImplementedException();
        }

        public static Vector3 GetAxisNormal(Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    return new Vector3(1, 0, 0);
                case Axis.Y:
                    return new Vector3(0, 1, 0);
                case Axis.Z:
                    return new Vector3(0, 0, 1);
                default:
                    break;
            }

            throw new System.NotImplementedException();
        }

        public static Quaternion GetQuaternionAxisRotation(Axis axis, float rotation)
        {
            return Quaternion.Euler(GetAxisRotation(axis, rotation));
        }

        public static Quaternion GetQuaternionReplacedAxisRotation(Axis axis, Quaternion quat, float rotation)
        {
            return Quaternion.Euler(GetReplacedAxisRotation(axis, quat.eulerAngles, rotation));
        }

        public Quaternion GetRotation()
        {
            return GetQuaternionAxisRotation(axis, angle);
        }
    }

    public Joint[] joints;

    public float angularSpeed = 15;

    private Vector3 localTarget;

    private Action<bool> completed;

    public bool IsBuisy
    {
        get
        {
            return completed != null;
        }
    }

    private static void GetEndPoint(Joint[] joints, out Quaternion endRotation, out Vector3 endPoint)
    {
        endRotation = joints[0].GetRotation();
        endPoint = Vector3.zero;

        int length = joints.Length;
        if (length == 0)
            return;

        for (int i = 1; i < length; i++)
        {
            endPoint = endPoint + (endRotation * joints[i - 1].offset);
            endRotation *= joints[i].GetRotation();
        }

        endPoint = endPoint + (endRotation * joints[length - 1].offset);
    }

    private static void GetEndPoint(Joint[] joints, int length, out Quaternion endRotation, out Vector3 endPoint)
    {
        endRotation = joints[0].GetRotation();
        endPoint = Vector3.zero;

        if (length == 0)
            return;

        for (int i = 1; i < length; i++)
        {
            endPoint = endPoint + (endRotation * joints[i - 1].offset);
            endRotation *= joints[i].GetRotation();
        }

        endPoint = endPoint + (endRotation * joints[length - 1].offset);
    }

    private static float GetEndPointDistance(Joint[] joints, Vector3 localTarget)
    {
        Quaternion computedEndRotation;
        Vector3 computedEndPoint;
        GetEndPoint(joints, out computedEndRotation, out computedEndPoint);

        return Vector3.Distance(computedEndPoint, localTarget);
    }

    private static float GetJointPartialGradient(Vector3 target, Joint[] joints, int angleIndex, float delta)
    {
        float currentY = GetEndPointDistance(joints, target);

        joints[angleIndex].angle += delta;
        float yWithDelta = GetEndPointDistance(joints, target);
        joints[angleIndex].angle -= delta;

        return (yWithDelta - currentY) / delta;//gradient
    }

    private static float[] GetJointsGradient(Vector3 target, Joint[] joints)
    {
        int length = joints.Length;
        if (length == 0)
            return null;

        float distance = GetEndPointDistance(joints, target);

        float[] results = new float[length];

        for (int i = 0; i < length; i++)
        {
            if (joints[i].doIk)
            {
                results[i] = GetJointPartialGradient(target, joints, i, 0.1f);
            }
        }

        return results;
    }

    private float DoIK(Vector3 localTarget, float maxSpeed)
    {
        float distance = GetEndPointDistance(joints, localTarget);

        const float acuracy = 0.01f;

        if (distance < acuracy)
            return 0;

        float[] gradient = GetJointsGradient(localTarget, joints);

        float change = 0;

        for (int i = 0; i < gradient.Length; i++)
        {
            if (joints[i].doIk == false)
                continue;
            float angle = Mathf.Clamp(distance * gradient[i], -maxSpeed, maxSpeed);
            float initial = joints[i].angle;
            joints[i].angle -= angle;

            if (joints[i].maxAngle - joints[i].minAngle < 360)
            {
                joints[i].angle = Mathf.Clamp(joints[i].angle, joints[i].minAngle, joints[i].maxAngle);
            }

            change += Mathf.Abs(Mathf.DeltaAngle(initial, joints[i].angle));

            if (GetEndPointDistance(joints, localTarget) < acuracy)
            {
                break;
            }
        }

        return change;
    }

    public void ReachPoint(Action<bool> completed, Vector3 localTarget)
    {
        this.completed = completed;
        this.localTarget = localTarget;
    }

    private void Update()
    {
        if (localTarget != Vector3.zero && completed != null)
        {
            float iterations = 100;
            float maxAngularSpeed = angularSpeed * Time.deltaTime;
            float maxIterationSpeed = maxAngularSpeed / iterations;

            Joint firstJoint = joints[0];
            firstJoint.doIk = false;
            Quaternion targetRot = Quaternion.LookRotation(localTarget);

            Debug.DrawLine(firstJoint.transform.position, firstJoint.transform.position + localTarget);

            // float targetAngle = Mathf.Clamp(targetRot.eulerAngles.y, firstJoint.minAngle, firstJoint.maxAngle);

            float last = firstJoint.angle;
            firstJoint.angle = Mathf.MoveTowardsAngle(firstJoint.angle, targetRot.eulerAngles.y, maxAngularSpeed);

            float change = Mathf.Abs(Mathf.DeltaAngle(last, firstJoint.angle));

            if (change < 0.01f)
            {
                for (int i = 0; i < iterations; i++)
                {
                    change += DoIK(localTarget, maxIterationSpeed);
                }
            }

            if (Mathf.Abs(change) < 0.01f)
            {
                float distance = GetEndPointDistance(joints, localTarget);
                if (distance > 0.01f)
                {
                    completed(false);
                    completed = null;
                }
                else
                {
                    completed(true);
                    completed = null;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        int length = joints.Length;

        if (length == 0)
            return;


        joints[0].transform.rotation = joints[0].GetRotation();
        Quaternion rotation = joints[0].transform.rotation;
        DrawJoint(Quaternion.identity, joints[0]);

        for (int i = 1; i < length; i++)
        {
            joints[i].transform.position = joints[i - 1].transform.position + (rotation * joints[i - 1].offset);
            DrawJoint(rotation, joints[i]);
            rotation *= joints[i].GetRotation();
            joints[i].transform.rotation = rotation;
        }

        UnityEditor.Handles.color = Color.blue;

        List<Vector3> points = joints.Select(a => a.transform.position).ToList();
        points.Add(joints[length - 1].transform.position + rotation * joints[length - 1].offset);

        UnityEditor.Handles.DrawAAPolyLine(6, points.ToArray());
    }

    private void DrawJoint(Quaternion rotation, Joint joint)
    {
        float angleSize = joint.maxAngle - joint.minAngle;
        Vector3 center = joint.transform.position;
        Vector3 normal = joint.transform.rotation * Joint.GetAxisRotation(joint.axis, 1);
        float radius = 0.3f;

        Vector3 currentAngle = joint.transform.rotation * Vector3.forward;
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawLine(center, center + (currentAngle * radius));

        Vector3 minAngle = rotation * Joint.GetQuaternionAxisRotation(joint.axis, joint.minAngle) * Vector3.forward;
        Vector3 maxAngle = rotation * Joint.GetQuaternionAxisRotation(joint.axis, joint.maxAngle) * Vector3.forward;
        UnityEditor.Handles.color = Color.green;
        UnityEditor.Handles.DrawLine(center, center + (minAngle * radius));
        UnityEditor.Handles.DrawLine(center, center + (maxAngle * radius));

        UnityEditor.Handles.color = new Color(0.1f, 0.5f, 0.1f, 0.2f);
        UnityEditor.Handles.DrawSolidArc(center, normal, minAngle, angleSize, radius);
    }
}
