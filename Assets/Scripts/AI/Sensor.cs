using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://github.com/SebLague/Field-of-View
[RequireComponent(typeof(SensorListener))]
public class Sensor : MonoBehaviour
{
    public float viewRadius;
    public float audibleRadius;

    // debug stuff
    public Vector3 eulerAngleToTarget = new Vector3(0, 0, 0);
    public bool targetObscured = false;

    [Range(0, 360)] public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [HideInInspector] public List<Transform> visibleTargets = new List<Transform>();
    [HideInInspector] public List<Transform> audibleTargets = new List<Transform>();

    private SensorListener[] sensorListeners;

    private Transform eyeTransform; // check for line of sight from this position

    void Start()
    {
        if(transform.Find("EyePosition") == null)
        {
            Debug.LogError("NPC does not have eye position!");
        }

        eyeTransform = transform.Find("EyePosition");
        sensorListeners = GetComponents<SensorListener>();
        StartCoroutine("FindTargetsWithDelay", .2f);
    }


    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
            FindAudibleTargets();
        }
    }

    // clamps angle to within viewAngle/2 degrees of 0/360
    float ClampToViewAngle(float angle)
    {
        float returnAngle = angle;
        if(returnAngle > viewAngle / 2 && returnAngle < 360 - viewAngle / 2)
        {
            if(360 - returnAngle < returnAngle)
            {
                // closer to 360
                returnAngle = 360 - viewAngle / 2;
            }
            else
            {
                // closer to 0
                returnAngle = viewAngle / 2;
            }
        }
        return returnAngle;
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Vector3 origin = eyeTransform.position;

        Collider[] targets = OverlapSphere(viewRadius);
        foreach (Collider target in targets)
        {
            Vector3 targetPosition = target.transform.position;

            // calculate angles to target on each axis, clamping each to viewAngle/2
            Quaternion angleToTarget = Quaternion.FromToRotation(eyeTransform.forward, targetPosition - origin);

            float clampedX, clampedY, clampedZ;
            clampedX = ClampToViewAngle(angleToTarget.eulerAngles.x);
            clampedY = ClampToViewAngle(angleToTarget.eulerAngles.y);
            clampedZ = ClampToViewAngle(angleToTarget.eulerAngles.z);

            angleToTarget.eulerAngles = new Vector3(0, clampedY, 0);
            eulerAngleToTarget = angleToTarget.eulerAngles;

            // check if the collider is hit by a raycast
            // this handles situations where the collider is overlapping the fov
            // but targetPosition is outside the cone
            Debug.DrawRay(origin, angleToTarget * (eyeTransform.forward * viewRadius), Color.magenta, 0.1f);
            RaycastHit targetHit, obstacleHit;
            if (Physics.Raycast(origin, angleToTarget * eyeTransform.forward, out targetHit, viewRadius, targetMask))
            {
                // check if there is an obstacle obscuring the target
                if (!Physics.Raycast(origin, angleToTarget * origin, out obstacleHit, viewRadius, obstacleMask))
                {
                    visibleTargets.Add(target.transform);
                    targetObscured = false;
                }
                else
                {
                    targetObscured = true;
                }
            }
        }

        if (visibleTargets.Count > 0)
        {
            foreach (var sensorListener in sensorListeners)
            {
                sensorListener.OnSpotted(visibleTargets[0].transform.position);
            }
        }
    }

    void FindAudibleTargets()
    {
        audibleTargets.Clear();
        
        var targets = OverlapSphere(audibleRadius);
        foreach (var target in targets)
        {     
            if (Vector3.Distance(eyeTransform.position, target.transform.position) < audibleRadius)
            {
                audibleTargets.Add(target.transform);
            }
        }

        if (audibleTargets.Count > 0)
        {
            foreach (var sensorListener in sensorListeners)
            {
                sensorListener.OnSoundHeard(audibleTargets[0].transform.position);
            }
        }
    }

    private Collider[] OverlapSphere(float radius)
    {
        return Physics.OverlapSphere(eyeTransform.position, radius, targetMask);
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}

public interface SensorListener
{
    void OnSpotted(Vector3 targetPosition);
    void OnSoundHeard(Vector3 targetPosition);
}