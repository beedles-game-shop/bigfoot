using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//---------------------------------------------------------------------------
//! This class changes the radius and length of a ranger's flashlight cone
//! to match the dimensions of the detection cone.

[RequireComponent(typeof(Light))]
public class FlashlightController : MonoBehaviour
{
    private Sensor sensor;
    private Light flashlight;
    private RangerController controller;

    //----------------------------------------------------------------
    //! Get references to sensor and light
    void Start()
    {
        sensor = transform.parent.GetComponent<Sensor>();
        flashlight = GetComponent<Light>();
        controller = transform.parent.GetComponent<RangerController>();

        if(sensor == null)
        {
            Debug.LogError("Flashlight assigned to a parent with no Sensor script!");
        }
        if (controller == null)
        {
            Debug.LogError("Flashlight assigned to a parent with no RangerController script!");
        }
    }

    //----------------------------------------------------------------
    //! Updates light
    void Update()
    {
        flashlight.range = controller.captureDistance;
        flashlight.innerSpotAngle = sensor.viewAngle * 1.0f;
        flashlight.spotAngle = sensor.viewAngle * 1.2f;
    }
}
