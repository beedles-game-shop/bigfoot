using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 3.25f;
    public float turnSpeed = 10f;
    public float grabRadius = 1;

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
        EventManager.TriggerEvent<ThoughtEvent, string, float>("I need a crate", 5.0f);
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
            heldObject.transform.position = gameObject.transform.position;
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
            heldObject.GetComponent<Collider>().enabled = true;
            heldObject.GetComponent<Rigidbody>().isKinematic = false;
            heldObject.GetComponent<Renderer>().enabled = true;

            EventManager.TriggerEvent<ItemDropEvent>();
            return;
        }

        // Check for reachable objects
        Collider[] reachableObjects = Physics.OverlapSphere(playerPosition, grabRadius);

        for (int i = 0; i < reachableObjects.Length; i++)
        {

            // If the object can be grabbed, pick it up
            if (reachableObjects[i].gameObject.tag == "Grab")
            {
                isHoldingObject = true;
                heldObject = reachableObjects[i].gameObject;
                heldObject.GetComponent<Collider>().enabled = false;
                heldObject.GetComponent<Rigidbody>().isKinematic = true;
                heldObject.GetComponent<Renderer>().enabled = false;
                EventManager.TriggerEvent<ItemGrabEvent, GameObject>(reachableObjects[i].gameObject);
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
            heldObject.GetComponent<Renderer>().enabled = true;
            heldObject.tag = "Untagged";

            EventManager.TriggerEvent<ItemDropEvent>();
            EventManager.TriggerEvent<ItemCollectEvent, GameObject>(heldObject);
        }
    }
}
