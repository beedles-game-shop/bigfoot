using UnityEngine;
using UnityEngine.AI;

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

    // Start is called before the first frame update
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

    // Update is called once per frame
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

    private void LateUpdate()
    {
        if (navAgent.velocity.sqrMagnitude > Mathf.Epsilon)
        {
            transform.rotation = Quaternion.LookRotation(navAgent.velocity.normalized);
        }
    }

    private void setNextWaypoint()
    {
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    private void setTarget()
    {
        navAgent.SetDestination(waypoints[currentWaypointIndex].transform.position);
    }

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

    public void OnSoundHeard(Vector3 targetPosition)
    {
    }

    public void CallForHelp(Vector3 helpPosition)
    {
        navAgent.SetDestination(helpPosition);
    }
}
