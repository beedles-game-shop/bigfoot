using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public void RetryButton()
    {
        SceneManager.LoadScene("Demo");
    }
    
    public void ExitButton() {
        Debug.Log("End");
        Application.Quit();
    }

}
