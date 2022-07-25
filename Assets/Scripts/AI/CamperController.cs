using UnityEngine;
using UnityEngine.AI;

//----------------------------------------------------------------
//! Controls fleeing if this camper sees the squatch
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class CamperController : MonoBehaviour, SensorListener
{
    public enum CamperState
    {
        Idling,
        HeardSomething,
        ReturningToStart,
        MovingToPointOfInterest,
        AtPointOfInterest,
        Fleeing,
        AtSafeSpace,
        Dead,
    }

    private GameObject exclamationPoint;
    private GameObject questionMark;

    private Alert Alert => new Alert(exclamationPoint, questionMark);

    NavMeshAgent navAgent;
    private Animator animator;

    private Vector3 fleeWaypoint;

    public float walkSpeed = 0.5f;
    public float runSpeed = 0.75f;

    public float fleeDistance = 10.0f;
    public bool isAggressive = false;

    private CamperState state;
    private Vector3 pointOfInterest;
    private float lastTimeHeardSec;
    private SensorListener.SensorSound lastHeard;
    private float secondsToRemainAlerted = 5f;
    private Vector3 startPosition;
    private float timeArrivedAtPOILocation;
    private float secondsToStayAtPOILocation = 5f;

    private CamperState State
    {
        get => state;
        set
        {
            if (state != value)
            {
                Debug.Log($"Camper: {state}->{value}");
            }
            state = value;
        }
    }

    //----------------------------------------------------------------
    //! Get references to necessary game objects
    protected void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.updateRotation = false;
        navAgent.updatePosition = false;

        animator = GetComponent<Animator>();

        exclamationPoint = transform.Find("ExclamationPoint").gameObject;
        if (exclamationPoint == null)
        {
            Debug.Log("Camper does not have exclamation point!");
        }

        questionMark = transform.Find("QuestionMark").gameObject;
        if (questionMark == null)
        {
            Debug.Log("Camper does not have question mark!");
        }

        State = CamperState.Idling;
        Alert.State = Alert.States.None;
        startPosition = transform.position;
        animator.SetBool("scared", false);
    }

    // Update is called once per frame
    protected void Update()
    {
        switch (State)
        {
            case CamperState.HeardSomething:
                animator.SetBool("scared", false);
                if (Time.realtimeSinceStartup - lastTimeHeardSec > secondsToRemainAlerted)
                {
                    Alert.State = Alert.States.None;
                    navAgent.speed = walkSpeed;
                    navAgent.SetDestination(startPosition);
                    State = CamperState.ReturningToStart;
                    break;
                }

                var look = lastHeard.TargetPosition - transform.position;
                look.y = 0;
                var rotation = Quaternion.LookRotation(look);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 0.8f);

                if (Quaternion.Angle(transform.rotation, rotation) < 20f)
                {
                    navAgent.SetDestination(pointOfInterest);
                    navAgent.speed = walkSpeed;
                    Alert.State = Alert.States.Question;
                    State = CamperState.MovingToPointOfInterest;
                }

                break;
            case CamperState.Fleeing:
                animator.SetBool("scared", false);
                Vector3 vectorToTarget = fleeWaypoint - transform.position;
                vectorToTarget.y = 0;

                if (vectorToTarget.magnitude - navAgent.stoppingDistance < 0.5f && !navAgent.pathPending)
                {
                    Alert.State = Alert.States.None;
                    navAgent.speed = 0.0f;
                    State = CamperState.AtSafeSpace;
                }
                break;
            case CamperState.AtSafeSpace:
                animator.SetBool("scared", true);
                break;
            case CamperState.AtPointOfInterest:
                animator.SetBool("scared", false);
                if (Time.realtimeSinceStartup - timeArrivedAtPOILocation > secondsToStayAtPOILocation)
                {
                    Alert.State = Alert.States.None;
                    navAgent.speed = walkSpeed;
                    navAgent.SetDestination(startPosition);
                    State = CamperState.ReturningToStart;
                }

                break;
            case CamperState.MovingToPointOfInterest:
                animator.SetBool("scared", false);
                if (Vector3.Distance(pointOfInterest, transform.position) - navAgent.stoppingDistance < 0.5f
                    && !navAgent.pathPending)
                {
                    Alert.State = Alert.States.None;
                    timeArrivedAtPOILocation = Time.realtimeSinceStartup;
                    navAgent.speed = 0.0f;
                    State = CamperState.AtPointOfInterest;
                }

                break;
            case CamperState.ReturningToStart:
                animator.SetBool("scared", false);
                if (Vector3.Distance(startPosition, transform.position) - navAgent.stoppingDistance < 0.5f
                    && !navAgent.pathPending)
                {
                    Alert.State = Alert.States.None;
                    navAgent.speed = 0.0f;
                    State = CamperState.Idling;
                }

                break;
            case CamperState.Idling:
            case CamperState.Dead:
                break;
        }

        if (animator.runtimeAnimatorController != null)
        {
            if (navAgent.velocity.magnitude > 0.1f)
            {
                animator.SetFloat("velY", navAgent.velocity.magnitude);
            }
            else
            {
                animator.SetFloat("velY", 0.0f);
            }
        }
    }

    void OnAnimatorMove()
    {
        Vector3 position = animator.rootPosition;
        position.y = navAgent.nextPosition.y;
        transform.position = position;
        navAgent.nextPosition = transform.position;

        if (navAgent.velocity.sqrMagnitude > 0.1f && State != CamperState.AtSafeSpace)
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
        switch (State)
        {
            case CamperState.MovingToPointOfInterest:
            case CamperState.ReturningToStart:
            case CamperState.Idling:
            case CamperState.AtPointOfInterest:
            case CamperState.AtSafeSpace:
            case CamperState.HeardSomething:
                Alert.State = Alert.States.Exclamation;
                alertNearestRanger();
                ChooseFleeWaypoint(targetPosition);
                navAgent.SetDestination(fleeWaypoint);
                navAgent.speed = runSpeed;
                EventManager.TriggerEvent<ThoughtEvent, string, float>("...!", 2.0f);
                State = CamperState.Fleeing;
                break;
            case CamperState.Fleeing:
                Vector3 vectorToTarget = fleeWaypoint - transform.position;
                vectorToTarget.y = 0;
                if (vectorToTarget.magnitude - navAgent.stoppingDistance < 0.5f && !navAgent.pathPending)
                {
                    State = CamperState.AtSafeSpace;
                }
                break;
            case CamperState.Dead:
                break;
        }
    }

    //----------------------------------------------------------------
    //! Called by the Sensor component if the squatch
    //! is within hearing radius of this camper. 
    //!
    //!     \param sensorSound information about the sound
    public void OnSoundHeard(SensorListener.SensorSound sensorSound)
    {
        switch (State)
        {
            case CamperState.MovingToPointOfInterest:
            case CamperState.ReturningToStart:
            case CamperState.AtPointOfInterest:
            case CamperState.AtSafeSpace:
            case CamperState.Idling:
                Alert.State = Alert.States.Question;
                EventManager.TriggerEvent<ThoughtEvent, string, float>("...", 2.0f);
                State = CamperState.HeardSomething;
                navAgent.speed = 0.0f;
                lastHeard = sensorSound;
                lastTimeHeardSec = Time.realtimeSinceStartup;
                pointOfInterest = sensorSound.TargetPosition;
                break;
            case CamperState.HeardSomething:
                lastTimeHeardSec = Time.realtimeSinceStartup;
                pointOfInterest = sensorSound.TargetPosition;
                break;
            case CamperState.Fleeing:
            case CamperState.Dead:
                break;
        }
        
    }

    //----------------------------------------------------------------
    //! Goes through all rangers on the map. Finds the nearest and 
    //! calls CallForHelp() on it.
    private void alertNearestRanger()
    {
        var allGameObjects = FindObjectsOfType<GameObject>();
        float closestRangerDistance = Mathf.Infinity;
        GameObject closestRanger = null;
        for (int i = 0; i < allGameObjects.Length; i++)
        {
            if (LayerMask.LayerToName(allGameObjects[i].layer) == "Ranger"
                && Vector3.Distance(transform.position, allGameObjects[i].transform.position) < closestRangerDistance)
            {
                closestRanger = allGameObjects[i];
            }
        }

        if (closestRanger != null)
        {
            closestRanger.GetComponent<RangerController>().CallForHelp(transform.position);
        }
        else
        {
            Debug.Log("Camper attempted to alert closest ranger, but none were found!");
        }
    }

    public void OnPhysical(Vector3 targetPosition)
    {
        switch (State)
        {
            case CamperState.AtPointOfInterest:
            case CamperState.MovingToPointOfInterest:
            case CamperState.ReturningToStart:
            case CamperState.Idling:
            case CamperState.HeardSomething:
            case CamperState.Fleeing:
            case CamperState.AtSafeSpace:
                EventManager.TriggerEvent<ThoughtEvent, string, float>("T.T", 8.0f);
                EventManager.TriggerEvent<FailedMenuEvent>();
                State = CamperState.Dead;
                break;
            case CamperState.Dead:
                break;
        }
    }

    void ChooseFleeWaypoint(Vector3 targetPosition)
    {
        // watch out
        if (isAggressive)
        {
            fleeWaypoint = targetPosition;
            return;
        }

        // get angle between bigfoot and self
        Vector3 fleeVector = new Vector3(transform.position.x - targetPosition.x, 1, transform.position.z - targetPosition.z);

        // choose spot to flee to
        fleeWaypoint = transform.position + fleeDistance * fleeVector.normalized;

        // if the waypoint is in an obstacle, rotate to the right until it is not
        bool keepLooking = true;
        while (keepLooking)
        {
            RaycastHit hitInfo;
            Ray pointRay = new Ray(fleeWaypoint, new Vector3(0, 0, 0));
            keepLooking = Physics.Raycast(pointRay, out hitInfo, 0);

            if (keepLooking)
            {
                fleeVector = Quaternion.AngleAxis(10, Vector3.up) * fleeVector;
                fleeWaypoint = transform.position + fleeDistance * fleeVector.normalized;
            }
        }
    }
}