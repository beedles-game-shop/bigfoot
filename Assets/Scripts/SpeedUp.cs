using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedUp : MonoBehaviour
{
    public float modifier = 10f;
    public float duration = 5;

    private bool isTriggered = false;
    private PlayerController playerController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerController = other.gameObject.GetComponent<PlayerController>();

            gameObject.SetActive(false);
            playerController.SetSpeedModifier(modifier, duration);
            isTriggered = true;
        }
    }
}
