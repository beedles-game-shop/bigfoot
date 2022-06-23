using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIEventLoader : MonoBehaviour
{
    public Image inventoryBlock;
    public GameObject gameOverPanel;
    public GameObject collectPanel;
    public List<GameObject> itemsToCollect;
    public GameObject victoryMessage;

    // List of sprites
    private Dictionary<string, Sprite> spriteMap;
    // List of items to collect
    private Dictionary<string, Image> collectImageMap;

    // Event Listeners
    private UnityAction<GameObject> itemGrabListener;
    private UnityAction<GameObject> itemCollectListener;
    private UnityAction itemDropListener;
    private UnityAction gameOverListener;

    void Awake()
    {   
        // Define listener's action handlers
        itemGrabListener = new UnityAction<GameObject>(ItemGrabHandler);
        itemCollectListener = new UnityAction<GameObject>(ItemCollectHandler);
        itemDropListener = new UnityAction(ItemDropHandler);
        gameOverListener = new UnityAction(GameOverHandler);

        // Map gameobject to sprite image
        spriteMap = new Dictionary<string, Sprite>(){
            {"cube", Resources.Load<Sprite>("Images/box")},
            {"ball", Resources.Load<Sprite>("Images/ball")},
            {"chair", Resources.Load<Sprite>("Images/chair")},
            {"table", Resources.Load<Sprite>("Images/table")}
        };

        // Build Collection Goal Images
        BuildCollectionList();
    }

    void OnEnable()
    {
        EventManager.StartListening<ItemGrabEvent, GameObject>(itemGrabListener);
        EventManager.StartListening<ItemCollectEvent, GameObject>(itemCollectListener);
        EventManager.StartListening<ItemDropEvent>(itemDropListener);
        EventManager.StartListening<GameOverEvent>(gameOverListener);
    }

    void OnDisable()
    {
        EventManager.StopListening<ItemGrabEvent, GameObject>(itemGrabListener);
        EventManager.StopListening<ItemCollectEvent, GameObject>(itemCollectListener);
        EventManager.StopListening<ItemDropEvent>(itemDropListener);
        EventManager.StopListening<GameOverEvent>(gameOverListener);
    }

    void ItemGrabHandler(GameObject gameObj)
    {
        Debug.Log("ItemGrabHandler");
        Debug.Log(gameObj.name);

        // Set default Sprite
        Sprite sprite = Resources.Load<Sprite>("Images/box");

        string key = gameObj.name.ToLower();
        // Check if sprite is set for game object
        if(!spriteMap.TryGetValue(key, out sprite)){
            Debug.Log("sprite not found in map");
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
            victoryMessage.GetComponent<Text>().enabled = true;
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

    void GameOverHandler()
    {
        Debug.Log("Game over");
        GameOver.previousScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("GameOver");
    }
}