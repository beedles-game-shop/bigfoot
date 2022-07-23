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
            alertNearestRanger();
        }
    }

    private void alertNearestRanger()
    {
        var allGameObjects = FindObjectsOfType<GameObject>();
        float closestRangerDistance = Mathf.Infinity;
        GameObject closestRanger = null;
        for (int i = 0; i < allGameObjects.Length; i++)
        {
            
            if (LayerMask.LayerToName(allGameObjects[i].layer) == "Ranger")
            {
                float dist = Vector3.Distance(transform.position, allGameObjects[i].transform.position);
                Debug.Log(dist);
                // only rangers within hearing range.
                if (dist <= soundRadius && dist < closestRangerDistance){
                    closestRanger = allGameObjects[i];
                }
                
            }
        }

        if (closestRanger != null)
        {
            // Trigger ranger warning
            closestRanger.GetComponent<RangerController>().CallForHelp(transform.position);
        }
        else
        {
            Debug.Log("Item attempted to alert closest ranger, but none were found!");
        }
    }
}
