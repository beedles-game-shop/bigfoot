using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SuccessMenu : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyUp (KeyCode.O)) {
            EventManager.TriggerEvent<SuccessMenuEvent>();
        } 
    }
    
    public void NextButton()
    {
        SceneManager.LoadScene("UIDemo1");
        Time.timeScale = 1f;
    }
    
    public void ExitButton() {
        Debug.Log("End");
        SceneManager.LoadScene("StartMenu");
        Time.timeScale = 1f;
    }
}
