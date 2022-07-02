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
        RespondingToCall,
        AtCall,
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
    public float navSpeedToAnimatorSpeedFactor = 0.5f;

    public GameObject[] waypoints;
    public float captureDistance = 3f;
    public float captureTimeSec = 1.0f;
    public float speed = 1f;
    public float secondsToRemainAlerted = 3f;

    private int currentWaypointIndex = -1;

    private Vector3 callForHelpLocation = new Vector3(0, 0, 0);
    public float secondsToStayAtCallForHelpLocation = 5f;
    private float timeArrivedAtHelpLocation = -1.0f;

    private GameObject exclamationPoint;
    private GameObject questionMark;
    private Alert Alert => new Alert(exclamationPoint, questionMark);

    private float lastTimeSpottedSec;
    private float timeCaptureEnteredSec = -1; // -1 if out of capture
    private SensorListener.SensorSound lastHeard;
    private float lastTimeHeardSec;

    public float distanceToTarget = -1;

    //----------------------------------------------------------------
    //! Get references to all necessary game components. Sets initial
    //! patrol point
    private void Start()
    {
        lastTimeSpottedSec = Time.realtimeSinceStartup;

        navAgent = GetComponent<NavMeshAgent>();
        navAgent.updateRotation = false;
        navAgent.speed = speed;

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
                    setTarget();
                }
                else
                {
                    StopWalkAnimation();
                }
                break;
            case RangerState.Chasing:
                StartWalkAnimation();

                if (Time.realtimeSinceStartup - lastTimeSpottedSec > secondsToRemainAlerted)
                {
                    Alert.State = Alert.States.None;
                    State = RangerState.Patrolling;
                }

                break;
            case RangerState.Capturing:
                StartWalkAnimation();

                if (Time.realtimeSinceStartup - timeCaptureEnteredSec > captureTimeSec)
                {
                    EventManager.TriggerEvent<FailedMenuEvent>();
                    State = RangerState.Dead;
                }

                break;
            case RangerState.HeardSomething:
                StopWalkAnimation();

                if (Time.realtimeSinceStartup - lastTimeHeardSec > secondsToRemainAlerted)
                {
                    Alert.State = Alert.States.None;
                    State = RangerState.Patrolling;
                    break;
                }
                
                var look = lastHeard.TargetPosition - transform.position;
                look.y = 0;
                var rotation = Quaternion.LookRotation(look);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 0.5f);
                
                break;
            case RangerState.RespondingToCall:
                StartWalkAnimation();

                Vector3 vectorToCall = callForHelpLocation - transform.position;
                vectorToCall.y = 0;
                if (vectorToCall.magnitude - navAgent.stoppingDistance < 0.5f && !navAgent.pathPending)
                {
                    timeArrivedAtHelpLocation = Time.realtimeSinceStartup;
                    StopWalkAnimation();
                    State = RangerState.AtCall;
                }
                break;
            case RangerState.AtCall:
                StopWalkAnimation();
                if (Time.realtimeSinceStartup - timeArrivedAtHelpLocation > secondsToStayAtCallForHelpLocation)
                {
                    Alert.State = Alert.States.None;
                    State = RangerState.Patrolling;
                }
                break;
            case RangerState.Dead:
                StopWalkAnimation();
                break;
        }
    }


    //----------------------------------------------------------------
    //! Adjusts animator and navAgent speeds and rotations.
    void FixedUpdate()
    {
        navAgent.speed = speed;

        if (navAgent.velocity.sqrMagnitude > Mathf.Epsilon)
        {
            transform.rotation = Quaternion.LookRotation(navAgent.velocity.normalized);
        }
    }

    void OnAnimatorMove()
    {
        // Update position to agent position
        transform.position = navAgent.nextPosition;
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
        if (animator.runtimeAnimatorController != null)
        {
            // to prevent prefab errors
            animator.SetFloat("velY", speed * navSpeedToAnimatorSpeedFactor);
        }
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
            case RangerState.RespondingToCall:
            case RangerState.AtCall:
                if (Vector3.Distance(targetPosition, transform.position) < captureDistance)
                {
                    EventManager.TriggerEvent<ThoughtEvent, string, float>("O.O", 2.0f);
                    Alert.State = Alert.States.Exclamation;
                    timeCaptureEnteredSec = Time.realtimeSinceStartup;
                    State = RangerState.Capturing;
                    break;
                }
                
                EventManager.TriggerEvent<ThoughtEvent, string, float>("...!", 2.0f);
                Alert.State = Alert.States.Exclamation;
                lastTimeSpottedSec = Time.realtimeSinceStartup;
                navAgent.SetDestination(targetPosition);
                navAgent.speed = speed;
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
                    EventManager.TriggerEvent<ThoughtEvent, string, float>("...!", 2.0f);
                    timeCaptureEnteredSec = -1;
                    Alert.State = Alert.States.Exclamation;
                    State = RangerState.Chasing;
                    break;
                }
                
                lastTimeSpottedSec = Time.realtimeSinceStartup;
                navAgent.SetDestination(targetPosition);
                navAgent.speed = speed;
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
            case RangerState.HeardSomething:
            case RangerState.RespondingToCall:
            case RangerState.AtCall:
                Alert.State = Alert.States.Question;
                navAgent.SetDestination(transform.position);
                lastHeard = sensorSound;
                lastTimeHeardSec = Time.realtimeSinceStartup;
                State = RangerState.HeardSomething;
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
            case RangerState.RespondingToCall:
            case RangerState.AtCall:
                EventManager.TriggerEvent<ThoughtEvent, string, float>("...", 2.0f);
                navAgent.SetDestination(helpPosition);
                Alert.State = Alert.States.Question;
                State = RangerState.RespondingToCall;
                callForHelpLocation = helpPosition;
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
        StopWalkAnimation();

        switch (State)
        {
            case RangerState.Patrolling:
            case RangerState.RespondingToCall:
            case RangerState.AtCall:
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

    private void StartWalkAnimation()
    {
        if (animator.runtimeAnimatorController != null)
        {
            animator.SetFloat("velY", speed * navSpeedToAnimatorSpeedFactor);
        }
    }

    private void StopWalkAnimation()
    {
        if (animator.runtimeAnimatorController != null)
        {
            animator.SetFloat("velY", 0.0f);
        }
    }
}
