using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MenuEventLoader : MonoBehaviour
{
    private UnityAction pauseMenuListener;
    private UnityAction successMenuListener;
    private UnityAction failedMenuListener;

    public CanvasGroup pauseCanvaGroup;
    public CanvasGroup successCanvaGroup;
    public CanvasGroup failedCanvaGroup;


    void Awake()
    {   
        // Define listener's action handlers
        pauseMenuListener = new UnityAction(PauseMenuHandler);
        successMenuListener = new UnityAction(SuccessMenuHandler);
        failedMenuListener = new UnityAction(FailedMenuHandler);
    }

    void OnEnable()
    {
        EventManager.StartListening<PauseMenuEvent>(pauseMenuListener);
        EventManager.StartListening<SuccessMenuEvent>(successMenuListener);
        EventManager.StartListening<FailedMenuEvent>(failedMenuListener);
    }

    void OnDisable()
    {
        EventManager.StopListening<PauseMenuEvent>(pauseMenuListener);
        EventManager.StopListening<SuccessMenuEvent>(successMenuListener);
        EventManager.StopListening<FailedMenuEvent>(failedMenuListener);
    }

    void PauseMenuHandler()
    {
        CanvasGroup cg = pauseCanvaGroup;
        if (cg.interactable) {
            cg.interactable = false;
            cg.blocksRaycasts = false;
            cg.alpha = 0f;
            Time.timeScale = 1f;
        } else {
            cg.interactable = true;
            cg.blocksRaycasts = true;
            cg.alpha = 1f;
            Time.timeScale = 0f;
        }
    }

    void SuccessMenuHandler()
    {   
        CanvasGroup cg = successCanvaGroup;
        if (cg.interactable) {
            cg.interactable = false;
            cg.blocksRaycasts = false;
            cg.alpha = 0f;
            Time.timeScale = 1f;
        } else {
            cg.interactable = true;
            cg.blocksRaycasts = true;
            cg.alpha = 1f;
            Time.timeScale = 0f;
        }
    }

    void FailedMenuHandler()
    {
        CanvasGroup cg = failedCanvaGroup;
        if (cg.interactable) {
            cg.interactable = false;
            cg.blocksRaycasts = false;
            cg.alpha = 0f;
            Time.timeScale = 1f;
        } else {
            cg.interactable = true;
            cg.blocksRaycasts = true;
            cg.alpha = 1f;
            Time.timeScale = 0f;
        }
    }
}