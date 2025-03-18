using Obi;
using UnityEngine;

public class weighboatscript : MonoBehaviour
{
    public int maxScoops = 10;
    //this references a list in liquidScript
    public int dishCompoundTypeIndex = -1;
    public int scoopsHeld = 0;

    public void addScoop(int compoundType){
        if (dishCompoundTypeIndex == compoundType || dishCompoundTypeIndex == -1){
            dishCompoundTypeIndex = compoundType; 
            scoopsHeld += 1; 
            foreach (Transform child in transform)
            {
                if(!child.gameObject.activeSelf){
                    child.gameObject.SetActive(true);
                    return;
                }
                
            }
        }   
    }
}
