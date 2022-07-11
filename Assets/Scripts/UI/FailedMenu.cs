using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FailedMenu : MonoBehaviour
{

    void Update()
    {
        if (Input.GetKeyUp (KeyCode.L)) {
            EventManager.TriggerEvent<FailedMenuEvent>();
        } 
    }

    public void RetryButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1f;
    }
    
    public void ExitButton() {
        Debug.Log("End");
        SceneManager.LoadScene("StartMenu");
        Time.timeScale = 1f;
    }
}
