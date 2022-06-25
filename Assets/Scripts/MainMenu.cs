using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void StartButton() {
        Debug.Log("Start");
        // Load next scene in scene order
        SceneManager.LoadScene("Level1");
    }

    public void LevelButton() {
        Debug.Log("Level");
        SceneManager.LoadScene("LevelMenu");
    }

    public void ExitButton() {
        Debug.Log("End");
        Application.Quit();
    }
}
