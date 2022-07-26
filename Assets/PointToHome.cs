using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PointToHome : MonoBehaviour
{
    public Vector3 destination;

    private GameObject player;
    private Vector3 vectorToCave;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        SetRotation();
        SetPosition();
    }

    // Rotates the object so that it points to the cave
    private void SetRotation()
    {
        vectorToCave = (player.transform.position - destination).normalized;
        float yRotation = Mathf.Atan(Mathf.Abs(vectorToCave.z / vectorToCave.x)) * (180 / Mathf.PI);

        if (vectorToCave.x > 0 && vectorToCave.z > 0)
        {
            yRotation *= -1;
        }
        else if (vectorToCave.x < 0 && vectorToCave.z < 0)
        {
            yRotation = 180 - yRotation;
        }
        else
        {
            yRotation += 180;
        }

        gameObject.transform.eulerAngles = new Vector3(90, yRotation, 0);
    }

    // Sets the position of the object so that it is between the player and the cave
    private void SetPosition()
    {
        gameObject.transform.position = new Vector3(player.transform.position.x - vectorToCave.x * 2f, 1, player.transform.position.z - vectorToCave.z * 2f);
    }
}
