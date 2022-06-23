using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//----------------------------------------------------------------
//! Controls fleeing if this camper sees the squatch
[RequireComponent(typeof(NavMeshAgent))]
public class CamperController : MonoBehaviour, SensorListener
{
    public Material alert;
    public Material notAlert;

    private new Renderer renderer;

    NavMeshAgent navAgent;
    public GameObject fleeWaypoint;

    //----------------------------------------------------------------
    //! Get references to necessary game objects
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

    //----------------------------------------------------------------
    //! Points the camper along its current velocity vector. This is
    //! to avoid an ice skating effect when slowly rotated by the nav
    //! agent.
    private void LateUpdate()
    {
        if (navAgent.velocity.sqrMagnitude > Mathf.Epsilon)
        {
            transform.rotation = Quaternion.LookRotation(navAgent.velocity.normalized);
        }
    }

    //----------------------------------------------------------------
    //! Called by the Sensor component if the squatch
    //! can be seen by this camper. Alerts the nearest ranger and
    //! flees!
    //!
    //!     \param targetPosition absolute position of the squatch
    public void OnSpotted(Vector3 targetPosition)
    {
        renderer.material = alert;
        alertNearestRanger();
        navAgent.SetDestination(fleeWaypoint.transform.position);
    }

    //----------------------------------------------------------------
    //! Called by the Sensor component if the squatch
    //! is within hearing radius of this camper. 
    //!
    //!     \param targetPosition absolute position of the squatch
    public void OnSoundHeard(Vector3 targetPosition)
    {
    }

    //----------------------------------------------------------------
    //! Goes through all rangers on the map. Finds the nearest and 
    //! calls CallForHelp() on it.
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
        }
        else
        {
            Debug.Log("Camper attempted to alert closest ranger, but none were found!");
        }
    }
}
