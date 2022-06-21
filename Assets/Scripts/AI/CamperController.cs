using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CamperController : MonoBehaviour, SensorListener
{
    public Material alert;
    public Material notAlert;

    private new Renderer renderer;

    NavMeshAgent navAgent;
    public GameObject fleeWaypoint;

    // Start is called before the first frame update
    protected void Start()
    {
        renderer = transform.Find("Body").GetComponent<Renderer>();

        navAgent = GetComponent<NavMeshAgent>();
        navAgent.updateRotation = false;
    }

    // Update is called once per frame
    protected void Update()
    {

    }

    private void LateUpdate()
    {
        if (navAgent.velocity.sqrMagnitude > Mathf.Epsilon)
        {
            transform.rotation = Quaternion.LookRotation(navAgent.velocity.normalized);
        }
    }

    public void OnSpotted(Vector3 targetPosition)
    {
        renderer.material = alert;
        alertNearestRanger();
    }
    
    public void OnSoundHeard(Vector3 targetPosition)
    {
    }

    private void alertNearestRanger()
    {
        var allGameObjects = FindObjectsOfType<GameObject>();
        float closestRangerDistance = Mathf.Infinity;
        GameObject closestRanger = null;
        for(int i = 0; i < allGameObjects.Length; i++)
        {
            if(LayerMask.LayerToName(allGameObjects[i].layer) == "Ranger" 
                && Vector3.Distance(transform.position, allGameObjects[i].transform.position) < closestRangerDistance)
            {
                closestRanger = allGameObjects[i];
            }
        }

        if(closestRanger != null)
        {
            closestRanger.GetComponent<RangerController>().CallForHelp(transform.position);
            navAgent.SetDestination(fleeWaypoint.transform.position);
        }
        else
        {
            Debug.Log("Camper attempted to alert closest ranger, but none were found!");
        }
    }
}
