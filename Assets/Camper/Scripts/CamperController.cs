using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamperController : MonoBehaviour, SensorListener
{
    public Material alert;

    private new Renderer renderer;
    
    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnSpotted()
    {
        renderer.material = alert;
    }
}
