using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;

    private Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        offset = player.transform.position - gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Called once at the end of every frame
    private void FixedUpdate()
    {
        gameObject.transform.position = player.transform.position - offset;
    }
}
