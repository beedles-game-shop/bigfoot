using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public GameObject grabTrigger;
    public float speed = 1;

    private Rigidbody rigidBody;
    private Vector2 movement;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Called once at the end of every frame
    private void FixedUpdate()
    {
        Vector3 translation = new Vector3(movement.x, 0.0f, movement.y);

        rigidBody.transform.Translate(speed * 0.01f * translation);
    }

    // Called when the input system fires an event
    private void OnMove(InputValue inputValue)
    {
        movement = inputValue.Get<Vector2>();
    }
}
