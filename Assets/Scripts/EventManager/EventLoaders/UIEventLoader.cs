using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIEventLoader : MonoBehaviour
{
    private struct TimeoutCallback
    {
        public Action Callback { get;  }
        public float Timeout { get; }

        public TimeoutCallback(Action callback, float timeout)
        {
            Callback = callback;
            Timeout = timeout;
        }
    }
    
    public Image inventoryBlock;
    public GameObject collectPanel;
    public List<GameObject> itemsToCollect;
    public Image thoughtImage;


    // List of sprites
    private Dictionary<string, Sprite> spriteMap;
    // List of items to collect
    private Dictionary<string, Image> collectImageMap;

    // Event Listeners
    private UnityAction<GameObject> itemGrabListener;
    private UnityAction<GameObject> itemCollectListener;
    private UnityAction itemDropListener;
    private UnityAction gameOverListener;
    private UnityAction<string, float> thoughtListener;

    private TimeoutCallback? thoughtBubbleTimeout;

    void Awake()
    {   
        // Define listener's action handlers
        itemGrabListener = new UnityAction<GameObject>(ItemGrabHandler);
        itemCollectListener = new UnityAction<GameObject>(ItemCollectHandler);
        itemDropListener = new UnityAction(ItemDropHandler);
        gameOverListener = new UnityAction(GameOverHandler);
        thoughtListener = new UnityAction<string, float>(ThoughtHandler);

        // Map gameobject to sprite image
        spriteMap = new Dictionary<string, Sprite>(){
            {"cube", Resources.Load<Sprite>("Images/box")},
            {"ball", Resources.Load<Sprite>("Images/ball")},
            {"chair", Resources.Load<Sprite>("Images/chair")},
            {"table", Resources.Load<Sprite>("Images/table")},
            {"lamp", Resources.Load<Sprite>("Images/lamp")},
            {"picture", Resources.Load<Sprite>("Images/picture")},
            {"box", Resources.Load<Sprite>("Images/box2")},
            {"flashlight-old", Resources.Load<Sprite>("Images/lamp")},
            {"supply-drop-fabric", Resources.Load<Sprite>("Images/box2")}
        };

        // Build Collection Goal Images
        BuildCollectionList();
    }

    private void Update()
    {
        if (thoughtBubbleTimeout?.Timeout < Time.realtimeSinceStartup)
        {
            thoughtBubbleTimeout?.Callback();
            thoughtBubbleTimeout = null;
        }
    }

    void OnEnable()
    {
        EventManager.StartListening<ItemGrabEvent, GameObject>(itemGrabListener);
        EventManager.StartListening<ItemCollectEvent, GameObject>(itemCollectListener);
        EventManager.StartListening<ItemDropEvent>(itemDropListener);
        EventManager.StartListening<GameOverEvent>(gameOverListener);
        EventManager.StartListening<ThoughtEvent, string, float>(thoughtListener);
    }

    void OnDisable()
    {
        EventManager.StopListening<ItemGrabEvent, GameObject>(itemGrabListener);
        EventManager.StopListening<ItemCollectEvent, GameObject>(itemCollectListener);
        EventManager.StopListening<ItemDropEvent>(itemDropListener);
        EventManager.StopListening<GameOverEvent>(gameOverListener);
        EventManager.StopListening<ThoughtEvent, string, float>(thoughtListener);
    }

    void ItemGrabHandler(GameObject gameObj)
    {
        Debug.Log("ItemGrabHandler");
        Debug.Log(gameObj.name);

        // Set default Sprite
        Sprite sprite = null;
        Image img;

        string key = gameObj.name.ToLower();
        
        // Check if sprite is set for game object
        if(!spriteMap.TryGetValue(key, out sprite)){
            sprite = Resources.Load<Sprite>("Images/box");
            Debug.Log("sprite not found in map");
        } 

        // If object is an item to collect
        if(collectImageMap.TryGetValue(key, out img)){
            EventManager.TriggerEvent<ThoughtEvent, string, float>("Let's go home", 5.0f);
        }

        inventoryBlock.sprite = sprite;
    }

    void ItemDropHandler()
    {
        Debug.Log("ItemDropHandler");
        inventoryBlock.sprite = null;
    }

    void ItemCollectHandler(GameObject gameObj)
    {
        Debug.Log("ItemCollectHandler");

        Image img;
        string key = gameObj.name.ToLower();

        // Check if sprite is set for game object
        if(collectImageMap.TryGetValue(key, out img)){
            Debug.Log("collectible");
            collectImageMap.Remove(key);
            img.color = new Color32(255,255,225,100);
        }

        // Check Win
        if(collectImageMap.Count < 1){
            EventManager.TriggerEvent<ThoughtEvent, string, float>("WOO HOO!!", 8.0f);
            EventManager.TriggerEvent<SuccessMenuEvent>();
        }
    }


    // Building the images for items to collect on game UI.
    void BuildCollectionList()
    {
        collectImageMap = new Dictionary<string, Image>();
        float setY = -50;

        foreach (GameObject g in itemsToCollect){
            Sprite sprite = null;

            // set tag to grabable item, in case it is not set
            g.tag = "Grab";

            // get sprite image for object
            string key = g.name.ToLower();
            Debug.Log(key);
            if(!spriteMap.TryGetValue(key, out sprite)){
                sprite = Resources.Load<Sprite>("Images/box");
                Debug.Log("sprite not found in map");
            }

            // generate new image in collect list
            GameObject newObj = new GameObject();
            Image newImg = newObj.AddComponent<Image>();
            newImg.sprite = sprite;
            newImg.rectTransform.sizeDelta = new Vector2(40, 40);

            RectTransform rt = newObj.GetComponent<RectTransform>();
            rt.SetParent(collectPanel.transform);
            rt.anchorMin = new Vector2(0.5f, 1.0f);
            rt.anchorMax = new Vector2(0.5f, 1.0f);
            rt.anchoredPosition = new Vector2(0, setY);
            setY -= 60;
            // newObj.name = "test";
            
            newObj.SetActive(true);

            collectImageMap.Add(key, newImg);
        }
    }

    void ThoughtHandler(string message, float delay)
    {
        thoughtImage.gameObject.SetActive(true);
        Text thoughtText = thoughtImage.gameObject.GetComponentsInChildren<Text>()[0];
        if (message.Length != 0) {
            thoughtText.text = message;
        } else {
            thoughtText.text = "I need a crate.";
        }

        thoughtBubbleTimeout = new TimeoutCallback(() =>
        {
            thoughtImage.gameObject.SetActive(false);
        }, delay + Time.realtimeSinceStartup);
    }

    void GameOverHandler()
    {
        Debug.Log("Game over");
        GameOver.previousScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("GameOver");
    }


}