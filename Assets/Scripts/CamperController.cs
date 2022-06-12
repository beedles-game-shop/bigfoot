using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamperController : MonoBehaviour, SensorListener
{
    public Material alert;
    public Material notAlert;

    private new Renderer renderer;

    private float lastTimeAlertedSec;
    private float secondsToRemainAlerted = 0.5f;

    // Start is called before the first frame update
    protected void Start()
    {
        renderer = transform.Find("Body").GetComponent<Renderer>();
        lastTimeAlertedSec = Time.realtimeSinceStartup;
    }

    // Update is called once per frame
    protected void Update()
    {
        if(Time.realtimeSinceStartup - lastTimeAlertedSec > secondsToRemainAlerted)
        {
            renderer.material = notAlert;
        }
    }

    public void OnSpotted()
    {
        renderer.material = alert;
        lastTimeAlertedSec = Time.realtimeSinceStartup;
    }
}
