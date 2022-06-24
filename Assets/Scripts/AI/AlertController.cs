using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertController : MonoBehaviour
{
    private GameObject mainCamera;
    public float angleToCamera = 0;

    // Gets reference to main camera
    void Start()
    {
        mainCamera = GameObject.FindWithTag("MainCamera");
        if(mainCamera == null)
        {
            Debug.Log("Tag MainCamera not found for an alert!");
        }
    }

    // rotates self so alert image is always flat-on to the camera
    void LateUpdate()
    {
        float zDiff = transform.parent.transform.position.z - mainCamera.transform.position.z;
        float xDiff = transform.parent.transform.position.x - mainCamera.transform.position.x;

        angleToCamera = 180 - Mathf.Rad2Deg * Mathf.Atan2(zDiff, xDiff);
        transform.rotation = Quaternion.Euler(0, angleToCamera + 90, 0);
    }
}
