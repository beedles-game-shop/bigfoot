using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : MonoBehaviour
{

    public float soundRange = 5f;
    private bool inAir = false;
    public void Thrown()
    {
        inAir = true;
        Debug.Log("item thrown");
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
                if (dist <= soundRange && dist < closestRangerDistance){
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

    void OnCollisionEnter(Collision collision)
    {

        if (collision.transform.gameObject.name == "Terrain")
        {
            
            // isGrounded = true;
            if (inAir) {
                Debug.Log("landing item");
                alertNearestRanger();
                inAir = false;
                //make sound
            }
        }
						
    }
}
