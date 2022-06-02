using UnityEngine;

public class PlayerController : MonoBehaviour {
	
    public float speed;

    private float movementX;
    private float movementY;

    private Rigidbody rb;

    void Start ()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Vector2 v = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical")).normalized * speed;
        movementX = v.x;
        movementY = v.y;
    }

    void FixedUpdate ()
    {
        Vector3 movement = new Vector3 (movementX, 0, movementY);
        rb.AddForce (movement * speed);    
    }
}