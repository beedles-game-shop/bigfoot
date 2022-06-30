using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//----------------------------------------------------------------
//! Controls fleeing if this camper sees the squatch
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class CamperController : MonoBehaviour, SensorListener
{
    private GameObject exclamationPoint;
    private GameObject questionMark;

    NavMeshAgent navAgent;
    private Animator animator;

    public GameObject fleeWaypoint;

    public float speed = 0.8f;

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

        exclamationPoint.SetActive(false);
        questionMark.SetActive(false);
    }

    // Update is called once per frame
    protected void Update()
    {

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
        alertNearestRanger();
        navAgent.SetDestination(fleeWaypoint.transform.position);
        exclamationPoint.SetActive(true);

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
    //!     \param targetPosition absolute position of the squatch
    public void OnSoundHeard(Vector3 targetPosition)
    {
        questionMark.SetActive(true);
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

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("I felt something!");
        OnSpotted(other.transform.position);
    }
}
