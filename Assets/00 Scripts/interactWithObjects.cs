using System.Collections;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample;
using Unity.Netcode;
using UnityEngine;


public class interactWithObjects : MonoBehaviour
{
    const float EYE_WASH_RANGE = 1.7f;
    
    
    public Transform playerCamera;
    public float range = 7f;

    public bool playerHoldingObject;

    pickUpObjects pos;
    playerMovement movementScript;

    multihandler multiHandlerScript;

    [Header("Eye Wash")]
    public Vector3 eyeOffset;
    doorScriptXAxis eyeWashPushHandle;
    Transform eyeWashStation;
    Transform eyeTargetSpot;
    public bool isNearEyeWash; bool previousNearEyeWash;
    public bool eyeWashRunning; bool previousEyeWashRunning;
    public bool isWashingEyes;
    public bool gogglesOn;
    public GameObject goggles;

    [Header("Vent Stuff")]
    public bool readyToDrag;
    public GameObject currentJointObject;
    public GameObject parentVentObject;
    public VentController parentVentScript;
    public Vector3 pivotPointForCurrentJoint;
    public Vector3 playerFirstContactOnJoint;
    public float distFromCameraForJoint;
    public Vector3 currentMousePosition;
    public float actualStartingJointAngle = Mathf.Infinity;
    public float startingAngle = Mathf.Infinity;
    public float currentAngle;
    public float angleDifference;
    public bool ventColliding;
    public int angleDirection3 = 1;
    public float past3;
    public int cantGoThisWay3;
    public int angleDirection4 = 1;
    public float past4;
    public int cantGoThisWay4;
    public bool readyToAssignDir3 = true;
    public bool readyToAssignDir4 = true;
    public bool cameOff;


    void Start()
    {
        pos = GetComponent<pickUpObjects>();
        movementScript = GetComponent<playerMovement>();

        eyeWashStation = GameObject.FindWithTag("EyeWashStation").transform;
        eyeTargetSpot = eyeWashStation.Find("Player Target Head Position");
        eyeWashPushHandle = eyeWashStation.Find("Hinge For Push").Find("Push Handle").GetComponent<doorScriptXAxis>();

        // For Help Text
        multiHandlerScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<multihandler>();
    }

    void Update()
    {
        playerHoldingObject = pos.other != null;
        CheckForInput();
        goggles.SetActive(gogglesOn);
    }

    void CheckForInput()
    {
        // Allow the player to open doors with E while holding an object if they manage to get it in line of sight, But if they click, ONLY drop the item

        // if (Input.GetKeyDown(KeyCode.E)){
        //     CheckForDoors();
        //     CheckForCabinets();
        //     CheckForTareButton();
        // }

        if (Input.GetMouseButtonDown(0) && !playerHoldingObject)
        {
            CheckForDoors();
            CheckForCabinets();
            CheckForTareButton();
            CheckForFaucets();
        }

        if (Input.GetMouseButton(0) && !playerHoldingObject)
            DragVentsAround();
        else {
            readyToDrag = true;
            currentJointObject = null;
            parentVentObject = null;
            parentVentScript = null;
            pivotPointForCurrentJoint = Vector3.zero;
            playerFirstContactOnJoint = Vector3.zero;
            distFromCameraForJoint = 0f;
            currentMousePosition = Vector3.zero;
            actualStartingJointAngle = Mathf.Infinity;
            startingAngle = Mathf.Infinity;
            currentAngle = 0f;
            angleDifference = 0f;
            ventColliding = false;
            readyToAssignDir3 = true;
            readyToAssignDir4 = true;
            angleDirection3 = 0;
            angleDirection4 = 0;

        }

        eyeWashStationStuff();


    }

    void DragVentsAround(){

        if (!readyToDrag){
            currentMousePosition = playerCamera.transform.position + playerCamera.forward * distFromCameraForJoint;
             
             
            ventColliding = parentVentScript.colliding;

            if (currentJointObject.name == "FIRST JOINT"){
                var pivotFrom = pivotPointForCurrentJoint;
                // pivotFrom.y = currentMousePosition.y; // Adjust height but keep XZ movement

                var mouseAt = currentMousePosition;

                Vector3 direction = (mouseAt - pivotFrom).normalized;
                
                if (actualStartingJointAngle == Mathf.Infinity)
                    actualStartingJointAngle = parentVentScript.FirstJointY; //////////

                float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                if (startingAngle == Mathf.Infinity)
                    startingAngle = angle;
                
                currentAngle = angle;

                angleDifference = Mathf.DeltaAngle(startingAngle, currentAngle);

                //////////
                parentVentScript.FirstJointY = actualStartingJointAngle + angleDifference;
            }

            if (currentJointObject.name == "SECOND JOINT"){
                var pivotFrom = pivotPointForCurrentJoint;
                // pivotFrom.y = currentMousePosition.y; // Adjust height but keep XZ movement

                var mouseAt = currentMousePosition;

                Vector3 direction = (mouseAt - pivotFrom).normalized;

                
                if (actualStartingJointAngle == Mathf.Infinity)
                    actualStartingJointAngle = parentVentScript.SecondJointX; //////////

                float angle = -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                if (startingAngle == Mathf.Infinity)
                    startingAngle = angle;
                
                currentAngle = angle;

                angleDifference = Mathf.DeltaAngle(startingAngle, currentAngle);

                //////////
                parentVentScript.SecondJointX = actualStartingJointAngle + angleDifference;
            }


            if (currentJointObject.name == "THIRD JOINT"){
                var pivotFrom = pivotPointForCurrentJoint;
                // pivotFrom.y = currentMousePosition.y; // Adjust height but keep XZ movement

                var mouseAt = currentMousePosition;

                Vector3 direction = (mouseAt - pivotFrom).normalized;

                

                
                if (actualStartingJointAngle == Mathf.Infinity)
                    actualStartingJointAngle = parentVentScript.ThirdJointX; //////////

                float angle = -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                if (startingAngle == Mathf.Infinity)
                    startingAngle = angle;
                
                currentAngle = angle;

                angleDifference = Mathf.DeltaAngle(startingAngle, currentAngle);

                //////////
                past3 = parentVentScript.ThirdJointX;

                
                if (!ventColliding)
                    parentVentScript.ThirdJointX = actualStartingJointAngle + angleDifference;
                if (ventColliding){
                    if (readyToAssignDir3 && angleDirection3 != 0) cantGoThisWay3 = angleDirection3;
                    readyToAssignDir3 = false;
                    if (angleDirection3 == -cantGoThisWay3)
                        parentVentScript.ThirdJointX = actualStartingJointAngle + angleDifference;
                }

                if (!readyToAssignDir3)
                    cameOff = !ventColliding;

            
                if (past3 > actualStartingJointAngle + angleDifference)
                    angleDirection3 = 1;
                else
                    angleDirection3 = -1;
                
            }

            if (currentJointObject.name == "FOURTH JOINT"){
                var pivotFrom = pivotPointForCurrentJoint;
                // pivotFrom.y = currentMousePosition.y; // Adjust height but keep XZ movement

                var mouseAt = currentMousePosition;

                Vector3 direction = (mouseAt - pivotFrom).normalized;

                
                if (actualStartingJointAngle == Mathf.Infinity)
                    actualStartingJointAngle = parentVentScript.FourthJointX; //////////

                float angle = -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                if (startingAngle == Mathf.Infinity)
                    startingAngle = angle;
                
                currentAngle = angle;

                angleDifference = Mathf.DeltaAngle(startingAngle, currentAngle);

                //////////
                // parentVentScript.FourthJointX = actualStartingJointAngle + angleDifference;
                past4 = parentVentScript.FourthJointX;

                
                if (!ventColliding)
                    parentVentScript.FourthJointX = actualStartingJointAngle + angleDifference;
                if (ventColliding){
                    if (readyToAssignDir4 && angleDirection4 != 0) cantGoThisWay4 = angleDirection4;
                    readyToAssignDir4 = false;
                    if (angleDirection4 == -cantGoThisWay4)
                        parentVentScript.FourthJointX = actualStartingJointAngle + angleDifference;
                }

                if (!readyToAssignDir4)
                    cameOff = !ventColliding;

            
                if (past4 > actualStartingJointAngle + angleDifference)
                    angleDirection4 = 1;
                else
                    angleDirection4 = -1;
            }
        }

        Ray forwardRay = new Ray(playerCamera.transform.position, playerCamera.forward);
        if (readyToDrag && Physics.Raycast(forwardRay, out RaycastHit hit, range))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject) // We hit a door
            {
                if (hitObject.name.EndsWith("JOINT")){
                    Debug.Log("WE HIT A VENT");

                    // if (hitObject.name == "FIRST JOINT"){
                        readyToDrag = false;
                        currentJointObject = hitObject;
                        parentVentObject = findVentObject(hitObject);
                        parentVentScript = parentVentObject.GetComponent<VentController>();
                        pivotPointForCurrentJoint = hitObject.transform.Find("PivotFrom").position;
                        playerFirstContactOnJoint = hit.point;
                        distFromCameraForJoint = Vector3.Distance(hit.point, playerCamera.transform.position);
                    // }

                }

            }
        }   
    }

    GameObject findVentObject(GameObject currentJoint){
        if (currentJoint.name == "FIRST JOINT")
            return currentJoint.transform.parent.gameObject;
        
        if (currentJoint.name == "SECOND JOINT")
            return currentJoint.transform.parent.parent.gameObject;
        
        if (currentJoint.name == "THIRD JOINT")
            return currentJoint.transform.parent.parent.parent.gameObject;

        if (currentJoint.name == "FOURTH JOINT")
            return currentJoint.transform.parent.parent.parent.parent.gameObject;
        
        return null;
    }


    void eyeWashStationStuff(){
        isNearEyeWash = Vector3.Distance(playerCamera.position, eyeTargetSpot.position) < EYE_WASH_RANGE;
        eyeWashRunning = !eyeWashPushHandle.doorIsClosed;

        if (isNearEyeWash && eyeWashRunning && !isWashingEyes){
            multiHandlerScript.setHelpText("Press E to Rinse Eyes.");
            
            if (Input.GetKeyDown(KeyCode.E)){
                if (multiHandlerScript.helpText.text == "") multiHandlerScript.setHelpText("Press E to Rinse Eyes.");
                StartCoroutine(rinseEyes());
            }       
        } else if ( (previousNearEyeWash && !isNearEyeWash) || (previousEyeWashRunning && !eyeWashRunning) )
            multiHandlerScript.setHelpText("");
            
        previousEyeWashRunning = eyeWashRunning;
        previousNearEyeWash = isNearEyeWash;
    }

    IEnumerator rinseEyes(){
        movementScript.canMove = false; movementScript.canTurn = false;
        isWashingEyes = true;
        eyeOffset = Vector3.Lerp(playerCamera.position, eyeTargetSpot.position, 0.9f) - playerCamera.position;
        yield return new WaitForSeconds(1.5f); // Wait for 2 seconds
        eyeOffset = Vector3.zero; // Reset to (0,0,0)
        isWashingEyes = false;
        movementScript.canMove = true; movementScript.canTurn = true;
    }









    void CheckForDoors()
    {
        Ray forwardRay = new Ray(playerCamera.transform.position, playerCamera.forward);
        if (Physics.Raycast(forwardRay, out RaycastHit hit, range))
        {
            doorScript doorScriptObject = hit.collider.GetComponent<doorScript>();

            if (doorScriptObject) // We hit a door
            {
                doorScriptObject.InteractWithThisDoor();
                
            }

            doorScriptXAxis doorScriptObjectX = hit.collider.GetComponent<doorScriptXAxis>();

            if (doorScriptObjectX) // We hit a door
            {
                doorScriptObjectX.InteractWithThisDoor();
                
            }
        }
    }

    void CheckForFaucets(){
        Ray forwardRay = new Ray(playerCamera.transform.position, playerCamera.forward);
        if (Physics.Raycast(forwardRay, out RaycastHit hit, range))
        {
            faucetHandleScript faucetObject = hit.collider.GetComponent<faucetHandleScript>();

            if (faucetObject) // We hit a door
            {
                    faucetObject.InteractWithThisFaucet();
                
            }
        }
    }

    void CheckForTareButton()
    {
        Ray forwardRay = new Ray(playerCamera.transform.position, playerCamera.forward);
        if (Physics.Raycast(forwardRay, out RaycastHit hit, range))
        {
            if (hit.collider.name == "Tare-Button") // We hit the Tare-Button
            {
                Transform tareButtonTransform = hit.collider.transform;
                Transform parent = tareButtonTransform.parent; // Get the parent of the Tare-Button

                // Find the sibling with the WeightScale script
                WeightScale weightScaleScript = parent.GetComponentInChildren<WeightScale>();
                if (weightScaleScript != null)
                {
                    weightScaleScript.Tare();
                }
                else
                {
                    Debug.LogError("No WeightScale script found in sibling objects of Tare-Button.");
                }
            }
        }
    }



    void CheckForCabinets()
    {
        Ray forwardRay = new Ray(playerCamera.transform.position, playerCamera.forward);
        if (Physics.Raycast(forwardRay, out RaycastHit hit, range))
        {
            cabinetScript cabinetObjectScript = hit.collider.GetComponent<cabinetScript>();

            if (cabinetObjectScript) // We hit a cabinet
            {
                    cabinetObjectScript.InteractWithThisCabinet();
                
            }
        }
    }

}
