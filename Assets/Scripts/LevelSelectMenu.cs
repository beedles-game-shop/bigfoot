using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public void Level1Button() {
        Debug.Log("Level 1");
        // Start Level 1
        SceneManager.LoadScene("Level1");
    }

    public void Level2Button() {
        Debug.Log("Level 2");
        // Start Level 2
        SceneManager.LoadScene("Level2");
    }

    public void Level3Button() {
        Debug.Log("Start");
        // Start Level 3
        SceneManager.LoadScene("Level3");
    }

    public void ExitButton() {
        Debug.Log("Back");
        SceneManager.LoadScene("StartMenu");
    }
}
