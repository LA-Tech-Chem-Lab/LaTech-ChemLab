using Obi;
using UnityEngine;
using System.Collections.Generic;

public class weighboatscript : MonoBehaviour
{
    public int maxScoops = 10;
    //this references a list in liquidScript
    public int dishCompoundTypeIndex = -1;
    public int scoopsHeld = 0;
    public List<float> densities = new List<float> {1.83f, 2.12f, 1f, 2.66f, 2.7f, 1.5f, 2.672f, 1.76f, 2.42f, 1.75f, 1.57f};
    public doCertainThingWith doCertainThingWithScript;

    void Start()
    {
        GameObject player = GameObject.Find("Player");
        doCertainThingWithScript = player.GetComponent<doCertainThingWith>();
    }

    public void addScoop(int compoundType){
        //the weigh boat does not have a compound type set yet or it matches the one in the scoopula
        doCertainThingWithScript.tryingToMixCompoundsInNonLiquidHolder = false;
        if (dishCompoundTypeIndex == compoundType || dishCompoundTypeIndex == -1){
            dishCompoundTypeIndex = compoundType; 
            scoopsHeld += 1; 
            //adds mass to the rigidbody to compensate
            GetComponent<Rigidbody>().mass += 0.7407f * densities[dishCompoundTypeIndex] / 1000;
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
}
