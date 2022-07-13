using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 3.25f;
    public float turnSpeed = 10f;
    public float grabRadius = 1;
    public GameObject hands;

    private Animator animator;
    private Vector2 movement;
    private bool isHoldingObject = false;
    private GameObject heldObject;
    private new Rigidbody rigidbody;
    private Quaternion previousRot = Quaternion.identity;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        EventManager.TriggerEvent<ThoughtEvent, string, float>("I should collect some objects.", 5.0f);
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("velX", transform.rotation.y - previousRot.y);
        previousRot = transform.rotation;
        animator.SetFloat("velY", movement.magnitude * speed);

    }

    // Called once at the end of every frame
    private void FixedUpdate()
    {
        // Move the player
        if (movement.magnitude > 0)
        {
            var rotation = Quaternion.LookRotation(new Vector3(movement.x, 0, movement.y));
            rigidbody.MoveRotation(Quaternion.Slerp(rigidbody.rotation, rotation, Time.deltaTime * turnSpeed));
            rigidbody.velocity = rigidbody.transform.forward * speed;
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
            heldObject.GetComponent<Collider>().enabled = false;
            heldObject.GetComponent<Rigidbody>().isKinematic = false;
            heldObject.GetComponent<Rigidbody>().useGravity = true;
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
            heldObject.transform.position = playerPosition + new Vector3(0, 0, grabRadius);
            heldObject.GetComponent<Collider>().enabled = false;
            heldObject.GetComponent<Rigidbody>().isKinematic = false;
            heldObject.GetComponent<Rigidbody>().useGravity = true;
            EventManager.TriggerEvent<ItemDropEvent>();

            //Stop the grab animation
            animator.SetBool("carrying", false);
            return;
        }


        // Check for reachable objects
        Collider[] reachableObjects = Physics.OverlapSphere(playerPosition, grabRadius);

        bool reachableExists = false;        
        for (int i = 0; i < reachableObjects.Length; i++)
        {
            Debug.Log("reachable Object: " + reachableObjects[i]);
            // If the object can be grabbed, pick it up
            if (reachableObjects[i].gameObject.tag == "Grab")
            {
                isHoldingObject = true;
                reachableExists = true;
                heldObject = reachableObjects[i].gameObject;
                Debug.Log(heldObject);
                heldObject.GetComponent<Collider>().enabled = false;
                heldObject.GetComponent<Rigidbody>().isKinematic = false;
                heldObject.GetComponent<Rigidbody>().useGravity = false;
                EventManager.TriggerEvent<ItemGrabEvent, GameObject>(reachableObjects[i].gameObject);

                //Play the carry animation
                animator.SetBool("carrying", true);
  
            }
            else if (!reachableExists)
            {
                EventManager.TriggerEvent<ThoughtEvent, string, float>("There isn't an object to carry.", 2.0f);
            }
        }
    }

    // Called when the player enters a trigger
    private void OnTriggerEnter(Collider other)
    {
        Vector3 playerPosition = gameObject.transform.position;

        // If the player enters the cave while holding an object,
        // drop the object and make it un-grabbable
        if (other.gameObject.tag == "Cave" && isHoldingObject)
        {
            isHoldingObject = false;
            heldObject.transform.position = playerPosition + new Vector3(0, 0, grabRadius);
            heldObject.GetComponent<Collider>().enabled = true;
            heldObject.GetComponent<Rigidbody>().isKinematic = false;
            heldObject.GetComponent<Rigidbody>().useGravity = true;
            //heldObject.tag = "Untagged";

            EventManager.TriggerEvent<ItemDropEvent>();
            EventManager.TriggerEvent<ItemCollectEvent, GameObject>(heldObject);

            //Stop the grab animation
            animator.SetBool("carrying", false);
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        
    }
}
