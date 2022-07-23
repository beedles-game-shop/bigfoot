using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedUp : MonoBehaviour
{
    public float modifier = 10f;
    public float duration = 5;

    private bool isTriggered = false;
    private PlayerController playerController;

    private void Update()
    {
        if (!isTriggered) return;

        print(duration);
        duration -= Time.deltaTime;

        if (duration <= 0)
        {
            playerController.maxSpeed -= modifier;
        }
    }
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
