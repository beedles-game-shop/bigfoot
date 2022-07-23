using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialPlayerController : MonoBehaviour
{
    public float maxSpeed = 3.25f;
    public float minSpeed = 1f;
    public float turnSpeed = 10f;
    public float grabRadius = 1;
    public GameObject hands;
    public float throwForce = 300f;
    public float itemMassImpact = 0.1f; // How strongly a held item's mass affects bigfoot's speed

    private Animator animator;
    private Vector2 movement;
    private bool isHoldingObject = false;
    private GameObject heldObject;
    private new Rigidbody rigidbody;
    private Quaternion previousRot = Quaternion.identity;
    private float currentSpeed;

    //Booleans for tutorial prompts
    private bool radioPrompted = false;
    private bool camperHinted = false;
    private bool obstacleHinted = false;
    private bool benchHinted = false;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        EventManager.TriggerEvent<ThoughtEvent, string, float>("Collect the items that I need without getting caught by the campers or rangers. We need to keep my existence a mystery!", 5.0f);
        currentSpeed = maxSpeed;
    }

    // Called once at the end of every frame
    private void FixedUpdate()
    {
        // Set the animation properties
        animator.SetFloat("velX", transform.rotation.y - previousRot.y);
        previousRot = transform.rotation;
        animator.SetFloat("velY", currentSpeed * movement.magnitude);

        // Move the player
        if (movement.magnitude > 0)
        {
            var rotation = Quaternion.LookRotation(new Vector3(movement.x, 0, movement.y));
            rigidbody.MoveRotation(Quaternion.Slerp(rigidbody.rotation, rotation, Time.deltaTime * turnSpeed));
            rigidbody.velocity = rigidbody.transform.forward * currentSpeed;
        }
        else
        {
            rigidbody.velocity = Vector3.zero;
        }

       

        // If an object is being held, move it with the player
        if (isHoldingObject)
        {
            heldObject.transform.position = GameObject.Find("mixamorig:RightHand").transform.position;
        }

        // If the held object is no longer with the player, change the state
        if (isHoldingObject && heldObject.transform.position!= GameObject.Find("mixamorig:RightHand").transform.position)
        {
            isHoldingObject = false;
            heldObject.layer = LayerMask.NameToLayer("Collectable");
            heldObject.GetComponent<Rigidbody>().isKinematic = false;
            EventManager.TriggerEvent<ItemDropEvent>();
        }

    }

    // Called when the input system fires a "Move" event
    private void OnMove(InputValue inputValue)
    {
        movement = inputValue.Get<Vector2>();
    }

    // Called when the input system fires a "Grab" event
    private void OnGrab()
    {
        Vector3 playerPosition = gameObject.transform.position;

        // If the player is holding an object, drop it
        if (isHoldingObject)
        {
            isHoldingObject = false;

            Vector3 up = new Vector3(0, 1.5f, 0);
            heldObject.transform.position = playerPosition + up + transform.forward * 1.5f;
            heldObject.layer = LayerMask.NameToLayer("Collectable");
            heldObject.GetComponent<Rigidbody>().isKinematic = false;
            currentSpeed = maxSpeed;

            Vector3 movement = new Vector3(0, 150, 0);
            movement = movement + (transform.forward * throwForce);

            heldObject.GetComponent<Rigidbody>().AddForce(movement);
            EventManager.TriggerEvent<ItemDropEvent>();

            ItemScript item = heldObject.GetComponent<ItemScript>();
            if (item != null) {
                item.Thrown();
            }

            //Stop the grab animation
            animator.SetBool("carrying", false);
            return;
        }


        // Check for reachable objects
        Collider[] reachableObjects = Physics.OverlapSphere(playerPosition, grabRadius);
        //Check if reachable objects is a radio


        for (int i = 0; i < reachableObjects.Length; i++)
        {
            Debug.Log("reachable Object: " + reachableObjects[i]);
            // If the object can be grabbed, pick it up
            if (reachableObjects[i].gameObject.tag == "Grab")
            {
                isHoldingObject = true;
                heldObject = reachableObjects[i].gameObject;
                Debug.Log(heldObject);
                heldObject.layer = LayerMask.NameToLayer("CarriedCollectable");
                heldObject.GetComponent<Rigidbody>().isKinematic = true;
                currentSpeed = Mathf.Max(
                    maxSpeed - (heldObject.GetComponent<Rigidbody>().mass * itemMassImpact),
                    minSpeed
                );
                EventManager.TriggerEvent<ItemGrabEvent, GameObject>(reachableObjects[i].gameObject);

                //Play the carry animation
                animator.SetBool("carrying", true);
                EventManager.TriggerEvent<ThoughtEvent, string, float>("Press the space bar again to throw the object. Throwing an object at a ranger or camper will only make them angry.", 3.0f);
                return;
            }


            if (reachableObjects[i].gameObject.tag == "Radio")
            {
                EventManager.TriggerEvent<RadioEvent, GameObject>(reachableObjects[i].gameObject);
                animator.SetBool("interacting", true);
                if (!radioPrompted)
                {
                    EventManager.TriggerEvent<ThoughtEvent, string, float>("Radios will distract campers and rangers to their sound. They're hidden around each level, so make sure you explore!", 5.0f);
                    radioPrompted = true;
                }
                return;
            }
        }
        
        EventManager.TriggerEvent<ThoughtEvent, string, float>("There isn't an object to carry.", 2.0f);
    }

    // Called when the player enters a trigger
    private void OnTriggerEnter(Collider other)
    {
        Vector3 playerPosition = gameObject.transform.position;

        // If the player is near the bench, trigger a speech bubble
        if (other.gameObject.tag == "Bench" && !benchHinted)
        {
            EventManager.TriggerEvent<ThoughtEvent, string, float>("We should check out the park bench, it looks like there might be something helpful there!", 2.0f);
            benchHinted = true;
        }
        // If the player is near the radio, trigger a speech bubble
        if (other.gameObject.tag == "Radio")
        {
            EventManager.TriggerEvent<ThoughtEvent, string, float>("Press space to interact with radios or other objects.", 2.0f);
            radioPrompted = true;
        }

        // If the player enters the cave while holding an object,
        // drop the object and make it un-grabbable
        if (other.gameObject.tag == "Cave" && isHoldingObject)
        {
            isHoldingObject = false;
            heldObject.transform.position = playerPosition + new Vector3(0, 0, grabRadius);
            heldObject.layer = LayerMask.NameToLayer("Collectable");
            heldObject.GetComponent<Rigidbody>().isKinematic = false;
            //heldObject.tag = "Untagged";

            EventManager.TriggerEvent<ItemDropEvent>();
            EventManager.TriggerEvent<ItemCollectEvent, GameObject>(heldObject);

            //Stop the grab animation
            animator.SetBool("carrying", false);
        }

        if (other.gameObject.tag == "ObstacleRadius" && !obstacleHinted)
        {
            EventManager.TriggerEvent<ThoughtEvent, string, float>(" Be careful! Running into branches and rocks will slow you down & attract nearby rangers.", 3.0f);
        }
        if(other.gameObject.tag == "Obstacle")
        {
            EventManager.TriggerEvent<TripEvent, GameObject>(other.gameObject);
        }
        if (other.gameObject.tag == "Campsite" && !camperHinted)
        {

            EventManager.TriggerEvent<ThoughtEvent, string, float>("Try to move quickly! If campers see you they will alert rangers to run over!", 3.0f);
            camperHinted = true;

        }
    }
    //animation function to stop Bigfoot pointing at the radios/objects
    public void StopPointing()
    {
        animator.SetBool("interacting", false);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        
    }
}
