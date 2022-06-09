using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void StartButton() {
        Debug.Log("Start");
        // Load next scene in scene order
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ExitButton() {
        Debug.Log("End");
        Application.Quit();
    }
}
