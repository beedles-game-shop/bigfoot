using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public static string previousScene = "Demo";

    public void RetryButton()
    {
        SceneManager.LoadScene(previousScene);
    }
    
    public void ExitButton() 
    {
        Debug.Log("End");
        Application.Quit();
    }

}
