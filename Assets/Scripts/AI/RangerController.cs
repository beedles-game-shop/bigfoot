using UnityEngine;
using UnityEngine.AI;

//----------------------------------------------------------------
//! This class controls ranger patrolling and
//! responding to calls for help from campers.

[RequireComponent(typeof(NavMeshAgent))]
public class RangerController : MonoBehaviour, SensorListener
{
    NavMeshAgent navAgent;
    public GameObject[] waypoints;
    public float captureDistance;
    
    private int currentWaypointIndex = -1;

    public Material alert;
    public Material notAlert;

    private new Renderer renderer;

    private float lastTimeAlertedSec;
    private float secondsToRemainAlerted = 0.5f;

    //----------------------------------------------------------------
    //! Get references to all necessary game components. Sets initial
    //! patrol point
    private void Start()
    {
        renderer = transform.Find("Body").GetComponent<Renderer>();
        lastTimeAlertedSec = Time.realtimeSinceStartup;

        navAgent = GetComponent<NavMeshAgent>();
        navAgent.updateRotation = false;

        if (waypoints.Length > 0)
        {
            setNextWaypoint();
            setTarget();
        }
    }

    //----------------------------------------------------------------
    //! Checks if the ranger should stop being alerted if enough time has passed
    //! since seeing the squatch. Chooses next patrol point if the current patrol
    //! point has been reached.
    private void Update()
    {
        if (Time.realtimeSinceStartup - lastTimeAlertedSec > secondsToRemainAlerted)
        {
            renderer.material = notAlert;
        }

        if (waypoints.Length > 0 
            && Vector3.Distance(waypoints[currentWaypointIndex].transform.position, transform.position) - navAgent.stoppingDistance < 1 
            && !navAgent.pathPending)
        {
            setNextWaypoint();
            setTarget();
        }
    }

    //----------------------------------------------------------------
    //! Points the ranger along its current velocity vector. This is
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
    //! Increments the active waypoint
    private void setNextWaypoint()
    {
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    //----------------------------------------------------------------
    //! Points the nav agent at the currently active waypoint
    private void setTarget()
    {
        navAgent.SetDestination(waypoints[currentWaypointIndex].transform.position);
    }

    //----------------------------------------------------------------
    //! Called by the Sensor component if the squatch
    //! can be seen by this ranger. Triggers game over if the squatch
    //! is within captureDistance
    //!
    //!     \param targetPosition absolute position of the squatch
    public void OnSpotted(Vector3 targetPosition)
    {
        if (Vector3.Distance(targetPosition, transform.position) < captureDistance)
        {
            EventManager.TriggerEvent<GameOverEvent>();
        }
        
        renderer.material = alert;
        lastTimeAlertedSec = Time.realtimeSinceStartup;
        navAgent.SetDestination(targetPosition);
    }

    //----------------------------------------------------------------
    //! Called by the Sensor component if the squatch
    //! is within hearing radius of this ranger. 
    //!
    //!     \param targetPosition absolute position of the squatch
    public void OnSoundHeard(Vector3 targetPosition)
    {
    }

    //----------------------------------------------------------------
    //! Called by a camper if the camper sees a squatch and this ranger
    //! is the closest. Directs the ranger to the help position
    //!
    //!     \param helpPosition absolute position of the camper calling for help
    public void CallForHelp(Vector3 helpPosition)
    {
        navAgent.SetDestination(helpPosition);
    }
}
