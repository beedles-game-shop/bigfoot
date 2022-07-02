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
        Fleeing,
        AtSafeSpace,
        Dead,
    }

    private GameObject exclamationPoint;
    private GameObject questionMark;

    private Alert Alert => new Alert(exclamationPoint, questionMark);

    NavMeshAgent navAgent;
    private Animator animator;
    public float navSpeedToAnimatorSpeedFactor = 0.5f;

    public GameObject fleeWaypoint;

    public float speed = 1f;

    private CamperState state;
    private CamperState State
    {
        get => state;
        set
        {
            Debug.Log($"Camper: {state}->{value}");
            state = value;
        }
    }

    //----------------------------------------------------------------
    //! Get references to necessary game objects
    protected void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.updateRotation = false;

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
    }

    // Update is called once per frame
    protected void Update()
    {
        animator.SetFloat("velX", 0.0f);

        switch (State)
        {
            case CamperState.Idling:
            case CamperState.HeardSomething:
                if (animator.runtimeAnimatorController != null)
                {
                    // to prevent prefab errors
                    animator.SetFloat("velY", 0.0f);
                }
                break;
            case CamperState.Fleeing:
                if (animator.runtimeAnimatorController != null)
                {
                    // to prevent prefab errors
                    animator.SetFloat("velY", speed * navSpeedToAnimatorSpeedFactor);
                }

                Vector3 vectorToTarget = fleeWaypoint.transform.position - transform.position;
                vectorToTarget.y = 0;
                if (vectorToTarget.magnitude - navAgent.stoppingDistance < 0.5f && !navAgent.pathPending)
                {
                    State = CamperState.AtSafeSpace;
                }
                break;
            case CamperState.AtSafeSpace:
                if (animator.runtimeAnimatorController != null)
                {
                    // to prevent prefab errors
                    animator.SetFloat("velY", 0.0f);
                }
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
    //! Called by the Sensor component if the squatch
    //! can be seen by this camper. Alerts the nearest ranger and
    //! flees!
    //!
    //!     \param targetPosition absolute position of the squatch
    public void OnSpotted(Vector3 targetPosition)
    {
        switch (State)
        {
            case CamperState.Idling:
            case CamperState.HeardSomething:
                Alert.State = Alert.States.Exclamation;
                alertNearestRanger();
                navAgent.SetDestination(fleeWaypoint.transform.position);
                State = CamperState.Fleeing;
                break;
            case CamperState.Fleeing:
                Vector3 vectorToTarget = fleeWaypoint.transform.position - transform.position;
                vectorToTarget.y = 0;
                if (vectorToTarget.magnitude - navAgent.stoppingDistance < 0.5f && !navAgent.pathPending)
                {
                    State = CamperState.AtSafeSpace;
                }
                break;
            case CamperState.AtSafeSpace:
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
            case CamperState.Idling:
            case CamperState.HeardSomething:
            case CamperState.Fleeing:
            case CamperState.AtSafeSpace:
                EventManager.TriggerEvent<FailedMenuEvent>();
                state = CamperState.Dead;
                break;
            case CamperState.Dead:
                break;
        }
    }
}