using UnityEngine;
using UnityEngine.AI;

//----------------------------------------------------------------
//! Controls fleeing if this camper sees the squatch
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class CamperController : MonoBehaviour, SensorListener
{
    public enum State
    {
        IDLING,
        HEARD_SOMETHING,
        SPOTTED,
        FLEEING,
    }

    private GameObject exclamationPoint;
    private GameObject questionMark;

    NavMeshAgent navAgent;
    private Animator animator;

    public GameObject fleeWaypoint;

    public float speed = 0.8f;

    private State _state;

    private State state
    {
        get => _state;
        set
        {
            _state = value;
            Debug.Log("Camper is now: " + _state);
        }
    }

    private Vector3 lastSeenPosition;
    private Vector3 lastHeardPosition;

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

        state = State.IDLING;
    }

    // Update is called once per frame
    protected void Update()
    {
        switch (state)
        {
            case State.IDLING:
                Idling();
                break;
            case State.HEARD_SOMETHING:
                HeardSomething();
                break;
            case State.SPOTTED:
                Spotted();
                break;
            case State.FLEEING:
                Fleeing();
                break;
        }
    }

    private void Idling()
    {
        exclamationPoint.SetActive(false);
        questionMark.SetActive(false);
    }

    private void HeardSomething()
    {
        exclamationPoint.SetActive(false);
        questionMark.SetActive(true);
        EventManager.TriggerEvent<ThoughtEvent, string, float>("...", 2.0f);
    }

    private void Spotted()
    {
        exclamationPoint.SetActive(true);
        questionMark.SetActive(false);
        alertNearestRanger();
        state = State.FLEEING;
        EventManager.TriggerEvent<ThoughtEvent, string, float>("...!", 2.0f);
    }

    private void Fleeing()
    {
        exclamationPoint.SetActive(true);
        questionMark.SetActive(false);
        navAgent.SetDestination(fleeWaypoint.transform.position);
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
        lastSeenPosition = targetPosition;
        state = State.SPOTTED;

        if (animator.runtimeAnimatorController != null)
        {
            // to prevent prefab errors
            animator.SetFloat("velY", speed / 4.75f);
        }
    }

    //----------------------------------------------------------------
    //! Called by the Sensor component if the squatch
    //! is within hearing radius of this camper. 
    //!
    //!     \param sensorSound information about the sound
    public void OnSoundHeard(SensorListener.SensorSound sensorSound)
    {
        lastHeardPosition = sensorSound.TargetPosition;
        switch (state)
        {
            case State.IDLING:
            case State.HEARD_SOMETHING:
                state = State.HEARD_SOMETHING;
                break;
            case State.SPOTTED:
            case State.FLEEING:
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
        Debug.Log("I felt something!");
        OnSpotted(targetPosition);
    }
}