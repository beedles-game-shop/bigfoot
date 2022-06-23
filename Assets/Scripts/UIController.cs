using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public List<GameObject> itemsToCollect;
    public GameObject victoryMessage;

    private bool isGameWon = false;

    // Start is called before the first frame update
    void Start()
    {
        // Mark items in the itemsToCollect list as grabbable
        foreach(GameObject item in itemsToCollect)
        {
            item.tag = "Grab";
        }
    }

    // Update is called once per frame
    void Update()
    {
        // If the game has been won, return
        if(isGameWon)
        {
            return;
        }

        // If there are not items to collect, show victory message
        if (itemsToCollect.Count == 0)
        {
            isGameWon = true;
            victoryMessage.GetComponent<Text>().enabled = true;

            return;
        }

        string checklist = "Items to Collect:\n";

        for (int i = 0; i < itemsToCollect.Count; i++)
        {
            // If the item is not grabbable, remove it from the list
            if(itemsToCollect[i].tag != "Grab")
            {
                itemsToCollect.RemoveAt(i);
                i--;

                continue;
            }

            checklist += "- " + itemsToCollect[i].name + "\n";

        }

        gameObject.GetComponent<Text>().text = checklist;
    }
}