using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerMaterial : MonoBehaviour
{
    public Material newMaterial;
    private Material originalMat;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //Make the cave transparent when Bigfoot enters
    private void OnTriggerEnter(Collider other)
    {
        foreach (Transform child in transform){
            originalMat = child.GetComponent<MeshRenderer>().material;
            child.GetComponent<MeshRenderer>().material = newMaterial;
        }

    }
    //Make the cave opaque when Bigfoot exits
    private void OnTriggerExit(Collider other)
    {
        Debug.Log(originalMat.name);
        foreach (Transform child in transform)
        {
            child.GetComponent<MeshRenderer>().material = originalMat;
        }
    }
}
