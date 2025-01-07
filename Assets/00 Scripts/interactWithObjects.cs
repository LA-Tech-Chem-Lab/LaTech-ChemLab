using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class interactWithObjects : MonoBehaviour
{
    public Transform playerCamera;
    public float range = 7f;

    void Update()
    {
        checkForInput();

    }




    void checkForInput()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)){
            checkForDoors();
            checkForCabinets();
        }
        
        
    }

    void checkForDoors(){
        Ray forwardRay = new Ray(playerCamera.transform.position, playerCamera.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(forwardRay, out hit, range)) {
            doorScript doorScriptObject = hit.collider.GetComponent<doorScript>();

            if (doorScriptObject) // We hit a door
            {
                doorScriptObject.InteractWithThisDoor();
            }
        }
    }

    void checkForCabinets(){
        Ray forwardRay = new Ray(playerCamera.transform.position, playerCamera.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(forwardRay, out hit, range)) {
            cabinetScript cabinetObjectScript = hit.collider.GetComponent<cabinetScript>();

            if (cabinetObjectScript) // We hit a cabinet
            {
                cabinetObjectScript.InteractWithThisCabinet();
            }
        }
    }



}
