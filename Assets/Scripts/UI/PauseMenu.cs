using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyUp (KeyCode.P)) {
            Debug.Log("P");
            EventManager.TriggerEvent<PauseMenuEvent>();
        } 
    }

    public void ContinueButton()
    {
        Debug.Log("Continue");
        EventManager.TriggerEvent<PauseMenuEvent>();
        Time.timeScale = 1f;
        return;
    }
    
    public void ExitButton() {
        Debug.Log("End");
        SceneManager.LoadScene("StartMenu");
        Time.timeScale = 1f;
        return;
    }

}
