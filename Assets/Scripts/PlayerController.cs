using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 1;
    public float grabRadius = 1;

    private Vector2 movement;
    private bool isHoldingObject = false;
    private GameObject heldObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Called once at the end of every frame
    private void FixedUpdate()
    {
        // Move the player
        Vector3 translation = new Vector3(movement.x, 0.0f, movement.y);

        gameObject.transform.Translate(speed * 0.01f * translation);

        // If an object is being held, move it with the player
        if(isHoldingObject)
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
            heldObject.GetComponent<BoxCollider>().enabled = true;
            heldObject.GetComponent<Rigidbody>().isKinematic = false;

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
                heldObject.GetComponent<BoxCollider>().enabled = false;
                heldObject.GetComponent<Rigidbody>().isKinematic = true;
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
            heldObject.GetComponent<BoxCollider>().enabled = true;
            heldObject.GetComponent<Rigidbody>().isKinematic = false;
            heldObject.tag = "Untagged";
        }
    }
}
