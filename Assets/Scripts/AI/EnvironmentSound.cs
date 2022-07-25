using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnvironmentSound : MonoBehaviour
{
    public float soundRadius = 10f;
    public LayerMask targetMask;
    private new Collider collider;

    private void Start()
    {
        collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((targetMask.value & (1 << other.gameObject.layer)) > 0)
        {
            EmitSoundEvent(other.bounds.center);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.gameObject.name == "Terrain")
        {
            EmitSoundEvent(collider.bounds.center);
        }
    }

    public void EmitSoundEvent(Vector3 position)
    {
        var sensors = FindObjectsOfType<Sensor>().Where(sensor =>
            Vector3.Distance(transform.position, sensor.transform.position) < soundRadius);
        foreach (var sensor in sensors)
        {
            sensor.OnEnvironmentalSound(position, gameObject.tag);
        }
    }
}
