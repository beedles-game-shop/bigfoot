using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://github.com/SebLague/Field-of-View
[RequireComponent(typeof(SensorListener))]
public class Sensor : MonoBehaviour
{
    public float viewRadius = 5f;
    public float audibleRadius = 3.5f;
    [Range(0, 360)] public float viewAngle = 90;

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
        StartCoroutine("FindTargetsWithDelay", .1f);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (targetMask != (targetMask | (1 << collider.gameObject.layer))) return;

        foreach (var sensorListener in sensorListeners)
        {
            sensorListener.OnPhysical(collider.bounds.center);
        }
    }

    public void OnEnvironmentalSound(Vector3 position, string tag)
    {
        audibleTargets.Clear();
        var origin = eyeTransform.position;
        var ray = TargetRay(origin, position, false);

        SensorListener.SensorSound sound;
        if (RaycastTarget(ray))
        {
            sound = new SensorListener.SensorSound(position, false, true, tag);
        }
        else
        {
            sound = new SensorListener.SensorSound(position, true, true, tag);
        }

        audibleTargets.Add(sound);

        foreach (var sensorListener in sensorListeners)
        {
            sensorListener.OnSoundHeard(sound);
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
        var targets = OverlapSphere(viewRadius);
        foreach (var target in targets)
        {
            var position = target.bounds.center;
            var ray = TargetRay(origin, position, true);
            Debug.DrawRay(ray.origin, ray.direction * viewRadius, Color.magenta);

            if (RaycastTarget(ray))
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

    private Ray TargetRay(Vector3 origin, Vector3 position, bool clamp)
    {
        var direction = (position - origin).normalized;
        var angle = Vector3.SignedAngle(direction, eyeTransform.forward, Vector3.down);
        if (clamp)
        {
            angle = Mathf.Clamp(angle, -viewAngle / 2, viewAngle / 2);
        }

        return new Ray(origin, DirFromAngle(angle, false));
    }

    private bool RaycastTarget(Ray ray)
    {
        RaycastHit hitInfo;
        var hit = Physics.Raycast(ray, out hitInfo, viewRadius, targetMask | obstacleMask);
        return hit && targetMask == (targetMask | (1 << hitInfo.transform.gameObject.layer));
    }

    void FindAudibleTargets()
    {
        audibleTargets.Clear();

        var origin = eyeTransform.position;
        var targets = OverlapSphere(audibleRadius);
        foreach (var target in targets)
        {
            var position = target.bounds.center;
            var ray = TargetRay(origin, position, false);

            if (RaycastTarget(ray))
            {
                audibleTargets.Add(new SensorListener.SensorSound(position, false, false, target.tag));
            }
            else
            {
                audibleTargets.Add(new SensorListener.SensorSound(position, true, false, target.tag));
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
        public SensorSound(Vector3 targetPosition, bool muted, bool environmental, string tag)
        {
            Muted = muted;
            TargetPosition = targetPosition;
            Environmental = environmental;
            Tag = tag;
        }

        // False when sound is line-of-sight
        public bool Muted { get; }

        // True when sound is triggered by the environment (stepping on a branch, etc.)
        public bool Environmental { get; }

        public string Tag { get; }

        public Vector3 TargetPosition { get; }
    }

    void OnSpotted(Vector3 targetPosition);
    void OnSoundHeard(SensorSound sensorSound);
    void OnPhysical(Vector3 targetPosition);
}