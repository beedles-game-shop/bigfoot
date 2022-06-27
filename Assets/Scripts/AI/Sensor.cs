using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://github.com/SebLague/Field-of-View
[RequireComponent(typeof(SensorListener))]
public class Sensor : MonoBehaviour
{
    public float viewRadius;
    public float audibleRadius;
    [Range(0, 360)] public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [HideInInspector] public List<Vector3> visibleTargets = new List<Vector3>();
    [HideInInspector] public List<Vector3> audibleTargets = new List<Vector3>();

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

    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        
        var targets = OverlapSphere(viewRadius);
        foreach (var target in targets)
        {
            var origin = eyeTransform.position;
            var position = target.ClosestPoint(origin);
            var direction = (position - origin).normalized;
            var maxDistance = Vector3.Distance(origin, position);
            
            if (Vector3.Angle(transform.forward, direction) < viewAngle / 2)
            {
                if (!Physics.Raycast(origin, direction, maxDistance, obstacleMask))
                {
                    visibleTargets.Add(position);
                }
            }
        }

        if (visibleTargets.Count > 0)
        {
            foreach (var sensorListener in sensorListeners)
            {
                sensorListener.OnSpotted(visibleTargets[0]);
            }
        }
    }

    void FindAudibleTargets()
    {
        audibleTargets.Clear();
        
        var targets = OverlapSphere(audibleRadius);
        foreach (var target in targets)
        {
            var origin = eyeTransform.position;
            var position = target.ClosestPoint(origin);
            var direction = (position - origin).normalized;
            var maxDistance = Vector3.Distance(origin, position);
            
            if (!Physics.Raycast(origin, direction, maxDistance, obstacleMask))
            {
                audibleTargets.Add(position);
            }
        }

        if (audibleTargets.Count > 0)
        {
            foreach (var sensorListener in sensorListeners)
            {
                sensorListener.OnSoundHeard(audibleTargets[0]);
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