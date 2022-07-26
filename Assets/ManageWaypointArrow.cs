using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageWaypointArrow : MonoBehaviour
{
    private GameObject waypointArrow;

    private void Start()
    {
        waypointArrow = GameObject.FindGameObjectWithTag("WaypointArrow");
        waypointArrow.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Cave")
        {
            DeactivateArrow();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Cave")
        {
            ActivateArrow();
        }
    }

    public void ActivateArrow()
    {
        waypointArrow.SetActive(true);
    }

    public void DeactivateArrow()
    {
        waypointArrow.SetActive(false);
    }
}
