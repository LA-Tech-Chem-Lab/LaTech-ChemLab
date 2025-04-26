using UnityEngine;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

public class WhiteboardController : MonoBehaviour
{
    // List to store all LiquidScript components
    private List<liquidScript> liquidScripts = new List<liquidScript>();
    private List<GameObject> canvasChildrenExcludingFirst = new List<GameObject>();
    private int onStep = 1;
    

    void Start()
    {
        // Find all GameObjects with the tag "liquidHolder"
        GameObject[] liquidHolders = GameObject.FindGameObjectsWithTag("LiquidHolder");

        // Loop through each one and try to get the LiquidScript component
        foreach (GameObject holder in liquidHolders)
        {
            liquidScript script = holder.GetComponent<liquidScript>();
            if (script != null)
            {
                liquidScripts.Add(script);
            }
            else
            {
                Debug.LogWarning("GameObject tagged 'liquidHolder' is missing a LiquidScript component: " + holder.name);
            }
        }
        CollectCanvasChildrenExceptFirst();
    }


    void Update()
    {
        foreach (liquidScript liquid in liquidScripts)
        {
            if (liquid.currReactionID == onStep){
                canvasChildrenExcludingFirst[onStep - 1].SetActive(true);
                onStep += 1; 
            }
            if (liquid.step3Done){
                canvasChildrenExcludingFirst[2].SetActive(true);
            }
            if (liquid.step4Done){
                canvasChildrenExcludingFirst[3].SetActive(true);
            }
        }
    }

    void CollectCanvasChildrenExceptFirst()
    {
        Transform canvasTransform = transform.Find("Canvas");
        if (canvasTransform == null)
        {
            Debug.LogWarning("Canvas child not found on " + gameObject.name);
            return;
        }

        int childCount = canvasTransform.childCount;
        if (childCount <= 1)
        {
            Debug.Log("Canvas has one or no children.");
            return;
        }

        for (int i = 1; i < childCount; i++) // start from 1 to skip the first child
        {
            GameObject child = canvasTransform.GetChild(i).gameObject;
            canvasChildrenExcludingFirst.Add(child);
        }
    }
}

