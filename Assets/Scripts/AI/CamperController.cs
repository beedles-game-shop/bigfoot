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
    }

    // Update is called once per frame
    protected void Update()
    {
        switch (State)
        {
            case CamperState.Idling:
            case CamperState.HeardSomething:
                navAgent.speed = 0.0f;
                break;
            case CamperState.Fleeing:
                navAgent.speed = speed;
                Vector3 vectorToTarget = fleeWaypoint.transform.position - transform.position;
                vectorToTarget.y = 0;

                if (vectorToTarget.magnitude - navAgent.stoppingDistance < 0.5f && !navAgent.pathPending)
                {
                    State = CamperState.AtSafeSpace;
                }
                break;
            case CamperState.AtSafeSpace:
                navAgent.speed = 0.0f;
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
            case CamperState.Idling:
            case CamperState.HeardSomething:
                Alert.State = Alert.States.Exclamation;
                alertNearestRanger();
                navAgent.SetDestination(fleeWaypoint.transform.position);
                EventManager.TriggerEvent<ThoughtEvent, string, float>("...!", 2.0f);
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
        EventManager.TriggerEvent<ThoughtEvent, string, float>("...", 2.0f);
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
                EventManager.TriggerEvent<ThoughtEvent, string, float>("T.T", 8.0f);
                EventManager.TriggerEvent<FailedMenuEvent>();
                state = CamperState.Dead;
                break;
            case CamperState.Dead:
                break;
        }
    }
}