using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIScript : MonoBehaviour
{
    public GameObject helpImage;
    private bool helpToggle;

    void Start()
    {
        helpImage.SetActive(false);
        helpToggle = false;
    }

    void Update()
    {
        if (Input.GetKeyUp (KeyCode.H)) {
            HelpButton();
        } 
    }

    public void HelpButton()
    {
        helpToggle = !helpToggle;
        helpImage.SetActive(helpToggle);
        EventSystem.current.SetSelectedGameObject(null);
    }
}

