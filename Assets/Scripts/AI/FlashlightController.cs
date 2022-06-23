using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//---------------------------------------------------------------------------
//! This class changes the radius and length of a ranger's flashlight cone
//! to match the dimensions of the detection cone.

[RequireComponent(typeof(Sensor))]
[RequireComponent(typeof(Light))]
[RequireComponent(typeof(RangerController))]
public class FlashlightController : MonoBehaviour
{
    private Sensor sensor;
    private Light flashlight;
    private RangerController controller;

    //----------------------------------------------------------------
    //! Get references to sensor and light
    void Start()
    {
        sensor = GetComponent<Sensor>();
        flashlight = GetComponent<Light>();
        controller = GetComponent<RangerController>();
    }

    //----------------------------------------------------------------
    //! Updates light
    void Update()
    {
        flashlight.range = controller.captureDistance * 1.2f; // add a little wiggle room
        flashlight.innerSpotAngle = sensor.viewAngle;
        flashlight.spotAngle = sensor.viewAngle;
    }
}
