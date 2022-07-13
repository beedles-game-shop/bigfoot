using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnvironmentSound : MonoBehaviour
{
    public float soundRadius = 10f;
    public LayerMask targetMask;

    private void OnTriggerEnter(Collider other)
    {
        if ((targetMask.value & (1 << other.gameObject.layer)) > 0)
        {
            var sensors = FindObjectsOfType<Sensor>().Where(sensor => 
                Vector3.Distance(transform.position, sensor.transform.position) < soundRadius);
            foreach (var sensor in sensors)
            {
                sensor.OnEnvironmentalSound(other.bounds.center, gameObject.tag);
            }
        }
    }
}
