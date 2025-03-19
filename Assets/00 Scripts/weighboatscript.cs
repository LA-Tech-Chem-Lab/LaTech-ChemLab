using Obi;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class weighboatscript : MonoBehaviour
{
    public int maxScoops = 10;
    //this references a list in liquidScript
    public int scoopsHeld = 0;
    public List<float> densities = new List<float> {1.83f, 2.12f, 1f, 2.66f, 2.7f, 1.5f, 2.672f, 1.76f, 2.42f, 1.75f, 1.57f};
    public List<float> solutionMakeup = new List<float> {0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f};
    public doCertainThingWith doCertainThingWithScript;
    public bool isPouring = false;
    public float density;

    void Start()
    {
        GameObject player = GameObject.Find("Player");
        doCertainThingWithScript = player.GetComponent<doCertainThingWith>();
    }

    public void addScoop(List<float> compoundType){
        //the weigh boat does not have a compound type set yet or it matches the one in the scoopula
        doCertainThingWithScript.tryingToMixCompoundsInNonLiquidHolder = false;
        if (compoundType.SequenceEqual(solutionMakeup) || solutionMakeup.All(num => num == 0f)){
            solutionMakeup = compoundType; 
            scoopsHeld += 1; 
            //adds mass to the rigidbody to compensate
            calculateDensity();
            GetComponent<Rigidbody>().mass += 0.7407f * density / 1000;
            foreach (Transform child in transform)
            {
                if(!child.gameObject.activeSelf){
                    child.gameObject.SetActive(true);
                    return;
                }

            }
        } 
        //the weigh boat compound type does not match that of the scoopula
        else{
            doCertainThingWithScript.tryingToMixCompoundsInNonLiquidHolder = true;
        }  
    }

    public void removeScoop(){
        if (scoopsHeld > 0){
            scoopsHeld -= 1;
            GetComponent<Rigidbody>().mass -= 0.7407f * density / 1000;
            foreach (Transform child in transform)
            {
                if(child.gameObject.activeSelf){
                    child.gameObject.SetActive(false);
                    return;
                }

            }
        }
    }

    void calculateDensity()
{
    float weightedDensitySum = 0f;

    for (int i = 0; i < densities.Count; i++)
    {
        weightedDensitySum += solutionMakeup[i] / densities[i]; // Weighting by fraction
    }

    if (weightedDensitySum > 0)
    {
        density = 1f / weightedDensitySum; // Harmonic mean of densities
    }
    else
    {
        density = 0f; // No valid solution
    }
}

}
