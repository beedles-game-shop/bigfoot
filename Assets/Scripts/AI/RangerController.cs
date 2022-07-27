using UnityEngine;
using UnityEngine.AI;

//----------------------------------------------------------------
//! This class controls ranger patrolling and
//! responding to calls for help from campers.

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class RangerController : MonoBehaviour, SensorListener
{
    public enum RangerState
    {
        Patrolling,
        Chasing,
        HeardSomething,
        MovingToPointOfInterest,
        AtPointOfInterest,
        Capturing,
        Dead,
    }

    private RangerState state;

    private RangerState State
    {
        get => state;
        set
        {
            if (state != value)
            {
                Debug.Log($"Ranger: {state}->{value}");
            }
            state = value;
        }
    }
    
    private NavMeshAgent navAgent;
    private Animator animator;

    public GameObject[] waypoints;
    public float captureDistance = 3f;
    public float captureTimeSec = 1.0f;
    public float walkSpeed = 1f;
    public float runSpeed = 1.5f;
    public float secondsToRemainAlerted = 3f;

    private int currentWaypointIndex = -1;

    private Vector3 pointOfInterest = new Vector3(0, 0, 0);
    public float secondsToStayAtPOILocation = 5f;
    private float timeArrivedAtPOILocation = -1.0f;

    private GameObject exclamationPoint;
    private GameObject questionMark;
    private Alert Alert => new Alert(exclamationPoint, questionMark);

    private float lastTimeSpottedSec;
    private float timeCaptureEnteredSec = -1; // -1 if out of capture
    private SensorListener.SensorSound lastHeard;
    private float lastTimeHeardSec;

    public float distanceToTarget = -1;
    private Sensor sensor;

    //----------------------------------------------------------------
    //! Get references to all necessary game components. Sets initial
    //! patrol point
    private void Start()
    {
        lastTimeSpottedSec = Time.realtimeSinceStartup;

        navAgent = GetComponent<NavMeshAgent>();
        navAgent.updateRotation = false;
        navAgent.updatePosition = false;
        navAgent.speed = walkSpeed;
        sensor = GetComponent<Sensor>();

        animator = GetComponent<Animator>();

        //animator.SetFloat("velX", 0);
        //animator.SetFloat("velY", 0);

        if (waypoints.Length > 0)
        {
            setNextWaypoint();
            setTarget();
        }

        exclamationPoint = transform.Find("ExclamationPoint").gameObject;
        if (exclamationPoint == null)
        {
            Debug.Log("Ranger does not have exclamation point!");
        }
        questionMark = transform.Find("QuestionMark").gameObject;
        if (questionMark == null)
        {
            Debug.Log("Ranger does not have question mark!");
        }

        Alert.State = Alert.States.None;
        State = RangerState.Patrolling;
    }

    //----------------------------------------------------------------
    //! Checks if the ranger should stop being alerted if enough time has passed
    //! since seeing the squatch. Chooses next patrol point if the current patrol
    //! point has been reached.
    private void Update()
    {
        switch (State)
        {
            case RangerState.Patrolling:
                if (waypoints.Length > 0)
                {
                    Vector3 vectorToTarget = waypoints[currentWaypointIndex].transform.position - transform.position;
                    vectorToTarget.y = 0;
                    if (vectorToTarget.magnitude - navAgent.stoppingDistance < 0.5f && !navAgent.pathPending)
                    {
                        setNextWaypoint();
                    }
                    navAgent.speed = walkSpeed;
                    setTarget();
                }
                else
                {
                    navAgent.speed = 0.0f;
                }
                break;
            case RangerState.Chasing:
                if (Time.realtimeSinceStartup - lastTimeSpottedSec > secondsToRemainAlerted)
                {
                    Alert.State = Alert.States.None;
                    navAgent.speed = walkSpeed;
                    State = RangerState.Patrolling;
                }

                break;
            case RangerState.Capturing:
                if (Time.realtimeSinceStartup - timeCaptureEnteredSec > captureTimeSec)
                {
                    EventManager.TriggerEvent<FailedMenuEvent>();
                    State = RangerState.Dead;
                }

                break;
            case RangerState.HeardSomething:
                if (Time.realtimeSinceStartup - lastTimeHeardSec > secondsToRemainAlerted)
                {
                    Alert.State = Alert.States.None;
                    navAgent.speed = walkSpeed;
                    State = RangerState.Patrolling;
                    break;
                }
                
                var look = lastHeard.TargetPosition - transform.position;
                look.y = 0;
                var rotation = Quaternion.LookRotation(look);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 0.8f);

                if (Quaternion.Angle(transform.rotation, rotation) < 20f)
                {
                    navAgent.SetDestination(pointOfInterest);
                    Alert.State = Alert.States.Question;
                    navAgent.speed = runSpeed;
                    State = RangerState.MovingToPointOfInterest;
                }
                
                break;
            case RangerState.MovingToPointOfInterest:
                Vector3 vectorToCall = navAgent.destination - transform.position;
                vectorToCall.y = 0;
                if (vectorToCall.magnitude - navAgent.stoppingDistance < 0.5f && !navAgent.pathPending)
                {
                    timeArrivedAtPOILocation = Time.realtimeSinceStartup;
                    navAgent.speed = 0.0f;
                    State = RangerState.AtPointOfInterest;
                }
                break;
            case RangerState.AtPointOfInterest:

                if (Time.realtimeSinceStartup - timeArrivedAtPOILocation > secondsToStayAtPOILocation)
                {
                    navAgent.speed = walkSpeed;
                    Alert.State = Alert.States.None;
                    State = RangerState.Patrolling;
                }
                break;
            case RangerState.Dead:
                navAgent.speed = 0.0f;
                break;
        }

        if (animator.runtimeAnimatorController != null)
        {
            if(navAgent.velocity.magnitude > 0.1f)
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
        if (navAgent.velocity.sqrMagnitude > 0.1 && State != RangerState.HeardSomething)
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
    //! is within captureDistance for longer than captureTimeSec.
    //!
    //!     \param targetPosition absolute position of the squatch
    public void OnSpotted(Vector3 targetPosition)
    {
        switch (State)
        {
            case RangerState.Patrolling:
            case RangerState.Chasing:
            case RangerState.HeardSomething:
            case RangerState.MovingToPointOfInterest:
            case RangerState.AtPointOfInterest:
                if (Vector3.Distance(targetPosition, transform.position) < captureDistance)
                {
                    EventManager.TriggerEvent<ThoughtEvent, string, float>("O.O", 2.0f);
                    Alert.State = Alert.States.Exclamation;
                    timeCaptureEnteredSec = Time.realtimeSinceStartup;
                    navAgent.speed = runSpeed;
                    State = RangerState.Capturing;
                    break;
                }
                
                //EventManager.TriggerEvent<ThoughtEvent, string, float>("...!", 2.0f);
                Alert.State = Alert.States.Exclamation;
                lastTimeSpottedSec = Time.realtimeSinceStartup;
                navAgent.SetDestination(targetPosition);
                navAgent.speed = runSpeed;
                State = RangerState.Chasing;
                break;
            case RangerState.Capturing:
                if (Time.realtimeSinceStartup - timeCaptureEnteredSec > captureTimeSec)
                {
                    EventManager.TriggerEvent<ThoughtEvent, string, float>("T.T", 8.0f);
                    EventManager.TriggerEvent<FailedMenuEvent>();
                    Alert.State = Alert.States.None;
                    State = RangerState.Dead;
                    break;
                }

                if (Vector3.Distance(targetPosition, transform.position) > captureDistance)
                {
                   //EventManager.TriggerEvent<ThoughtEvent, string, float>("...!", 2.0f);
                    timeCaptureEnteredSec = -1;
                    Alert.State = Alert.States.Exclamation;
                    navAgent.speed = runSpeed;
                    State = RangerState.Chasing;
                    break;
                }
                
                lastTimeSpottedSec = Time.realtimeSinceStartup;
                navAgent.SetDestination(targetPosition);
                break;
            case RangerState.Dead:
                break;
        }
    }

    //----------------------------------------------------------------
    //! Called by the Sensor component if the squatch
    //! is within hearing radius of this ranger. 
    //!
    //!     \param sensorSound information about hte sound
    public void OnSoundHeard(SensorListener.SensorSound sensorSound)
    {
        switch (State)
        {
            case RangerState.Patrolling:
            case RangerState.AtPointOfInterest:
                Alert.State = Alert.States.Question;
                //EventManager.TriggerEvent<ThoughtEvent, string, float>("...", 2.0f);
                State = RangerState.HeardSomething;
                navAgent.speed = 0.0f;
                lastHeard = sensorSound;
                lastTimeHeardSec = Time.realtimeSinceStartup;
                pointOfInterest = sensorSound.TargetPosition;
                break;
            case RangerState.HeardSomething:
            case RangerState.MovingToPointOfInterest:
                lastHeard = sensorSound;
                lastTimeHeardSec = Time.realtimeSinceStartup;
                pointOfInterest = sensorSound.TargetPosition;
                break;
            case RangerState.Chasing:
            case RangerState.Capturing:
            case RangerState.Dead:
                break;
        }
    }

    //----------------------------------------------------------------
    //! Called by a camper if the camper sees a squatch and this ranger
    //! is the closest. Directs the ranger to the help position
    //!
    //!     \param helpPosition absolute position of the camper calling for help
    public void CallForHelp(Vector3 helpPosition)
    {
        switch (State)
        {
            case RangerState.Patrolling:
            case RangerState.MovingToPointOfInterest:
            case RangerState.AtPointOfInterest:
                //EventManager.TriggerEvent<ThoughtEvent, string, float>("...", 2.0f);
                navAgent.SetDestination(helpPosition);
                navAgent.speed = runSpeed;
                Alert.State = Alert.States.Question;
                State = RangerState.MovingToPointOfInterest;
                pointOfInterest = helpPosition;
                break;
            case RangerState.HeardSomething:
            case RangerState.Chasing:
            case RangerState.Capturing:
            case RangerState.Dead:
                break;
        }
    }

    public void OnPhysical(Vector3 targetPosition)
    {
        switch (State)
        {
            case RangerState.Patrolling:
            case RangerState.MovingToPointOfInterest:
            case RangerState.AtPointOfInterest:
            case RangerState.HeardSomething:
            case RangerState.Chasing:
            case RangerState.Capturing:
                EventManager.TriggerEvent<ThoughtEvent, string, float>("T.T", 8.0f);
                EventManager.TriggerEvent<FailedMenuEvent>();
                State = RangerState.Dead;               
                break;
            case RangerState.Dead:
                break;
        }
    }
}
