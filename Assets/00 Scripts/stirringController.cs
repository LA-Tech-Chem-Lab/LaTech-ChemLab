using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class stirringController : MonoBehaviour
{
    private doCertainThingWith playerScript;
    public Camera playerCamera;
    private bool stirringTrigger;
    public bool debugFlag = true;
    public Animator stirRodAnimator;

    /*
    void Start() // does this only execute once?
    {
        playerScript = GetComponent<doCertainThingWith>();

        //StartCoroutine(WaitForCamera());
        //playerCamera = GameObject.Find("Camera")?.GetComponent<Camera>();
        //if (playerCamera == null && debugFlag == true)
        //{
        //    Debug.LogError("Can't find camera");
        //}
        playerCamera = Camera.main;
    }

    IEnumerator WaitForCamera()
    {
        yield return new WaitUntil(() => Camera.main != null);
        playerCamera = Camera.main;
        Debug.Log("Found camera");
    }

    void Update(){   
        if (playerScript != null) {
            stirringTrigger = playerScript.beginStirring;
        }

        if (stirringTrigger == true) {
            Debug.Log("Stirring Trigger is TRUE");
            if (playerScript != null) {
                Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                RaycastHit hit;

                // cast a ray
                if (Physics.Raycast(ray, out hit, 5f)){
                    Debug.Log("A ray was cast");
                    // check if the ray is cast on a beaker
                    if (hit.collider.gameObject.name == "Beaker") {
                        Debug.Log("Ray collided with beaker");
                        // find stir rod child
                        Transform stirRodChild = hit.transform.Find("Stir Rod");
                        Debug.Log("Stir Rod Layer: " + LayerMask.LayerToName(stirRodChild.gameObject.layer));

                        if (stirRodChild != null){
                            stirRodAnimator = stirRodChild.GetComponent<Animator>();

                            if (stirRodAnimator != null){
                                stirRodAnimator.SetTrigger("StartStirring");
                            }
                        }
                    }
                }
            }
        } else {
            stirRodAnimator.ResetTrigger("StartStirring");
        }
    }
*/  /*void Start() {
        stirRodAnimator = GetComponent<Animator>();
    }

    void Update() {
        if (playerScript != null) {
            stirringTrigger = playerScript.beginStirring;
        }
        
        if (stirringTrigger) {
            stirRodAnimator.SetTrigger("StartStirring");
        } else {
            stirRodAnimator.ResetTrigger("StartStirring");
        }
    }
    */
}



// We need to access only beakers that have a child stir rod using raycast 

// first check if raycast is on the beaker
    // check if that beaker has a child && that child is a stir rod
        // check if stir rod has an animation component 
            // get that component from the stir rod