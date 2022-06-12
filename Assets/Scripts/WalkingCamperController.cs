using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class WalkingCamperController : CamperController
{
    NavMeshAgent navAgent;
    public GameObject[] waypoints;
    private int currentWaypointIndex = -1;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        navAgent = GetComponent<NavMeshAgent>();

        setNextWaypoint();
        setTarget();
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();

        if (Vector3.Distance(waypoints[currentWaypointIndex].transform.position, transform.position) - navAgent.stoppingDistance < 1 && !navAgent.pathPending)
        {
            setNextWaypoint();
            setTarget();
        }
    }

    private void setNextWaypoint()
    {
        if (waypoints.Length > 0)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
        else
        {
            Debug.Log("WalkingCamper: no waypoints set!");
        }
    }

    private void setTarget()
    {
        navAgent.SetDestination(waypoints[currentWaypointIndex].transform.position);
    }
}
