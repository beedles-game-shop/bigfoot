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
    [HideInInspector] public List<SensorListener.SensorSound> audibleTargets = new List<SensorListener.SensorSound>();

    private SensorListener[] sensorListeners;

    private Transform eyeTransform; // check for line of sight from this position

    void Start()
    {
        if (transform.Find("EyePosition") == null)
        {
            Debug.LogError("NPC does not have eye position!");
        }

        eyeTransform = transform.Find("EyePosition");
        sensorListeners = GetComponents<SensorListener>();
        StartCoroutine("FindTargetsWithDelay", .2f);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (targetMask != (targetMask | (1 << collider.gameObject.layer))) return;

        foreach (var sensorListener in sensorListeners)
        {
            sensorListener.OnPhysical(collider.bounds.center);
        }
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

        var origin = eyeTransform.position;

        // One of the sides of the fov cone
        var edge1 = new Ray(origin, DirFromAngle(-viewAngle / 2, false));
        // The other side of the fov cone
        var edge2 = new Ray(origin, DirFromAngle(viewAngle / 2, false));

        var targets = OverlapSphere(viewRadius);
        foreach (var target in targets)
        {
            var position = target.bounds.center;
            var direction = (position - origin).normalized;
            var targetRay = new Ray(origin, direction);

            var isWithinAngle = Vector3.Angle(eyeTransform.forward, direction) < viewAngle / 2;

            if (isWithinAngle && !RaycastObstacle(targetRay))
            {
                visibleTargets.Add(position);
            }
            else if (RaycastTarget(edge1) || RaycastTarget(edge2))
            {
                visibleTargets.Add(position);
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

    private bool RaycastTarget(Ray ray)
    {
        RaycastHit hitInfo;
        var hit = Physics.Raycast(ray, out hitInfo, viewRadius, targetMask | obstacleMask);
        return hit && targetMask == (targetMask | (1 << hitInfo.transform.gameObject.layer));
    }

    private bool RaycastObstacle(Ray ray)
    {
        return Physics.Raycast(ray, viewRadius, obstacleMask);
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
                audibleTargets.Add(new SensorListener.SensorSound(position, false));
            }
            else
            {
                audibleTargets.Add(new SensorListener.SensorSound(position, true));
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
    public struct SensorSound
    {
        public SensorSound(Vector3 targetPosition, bool muted)
        {
            Muted = muted;
            TargetPosition = targetPosition;
        }

        public bool Muted { get; }

        public Vector3 TargetPosition { get; }
    }

    void OnSpotted(Vector3 targetPosition);
    void OnSoundHeard(SensorSound sensorSound);
    void OnPhysical(Vector3 targetPosition);
}