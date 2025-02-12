using System.Collections;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample;
using Unity.Netcode;
using UnityEngine;


public class interactWithObjects : NetworkBehaviour
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
        if (IsOwner)
        {
            playerHoldingObject = pos.other != null;
            CheckForInput();
        }
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

        eyeWashStationStuff();


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
                if (IsServer)
                {
                    doorScriptObject.InteractWithThisDoor();
                }
                else
                {
                    RequestDoorInteractionServerRpc(doorScriptObject.GetComponent<NetworkObject>().NetworkObjectId);
                }
            }

            doorScriptXAxis doorScriptObjectX = hit.collider.GetComponent<doorScriptXAxis>();

            if (doorScriptObjectX) // We hit a door
            {
                if (IsServer)
                {
                    doorScriptObjectX.InteractWithThisDoor();
                }
                else
                {
                    RequestDoorXAxisInteractionServerRpc(doorScriptObjectX.GetComponent<NetworkObject>().NetworkObjectId);
                }
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
                if (IsServer)
                {
                    faucetObject.InteractWithThisFaucet();
                }
                else
                {
                    RequestFaucetInteractionServerRpc(faucetObject.GetComponent<NetworkObject>().NetworkObjectId);
                }
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
                    weightScaleScript.RequestTareServerRpc();
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
                if (IsServer)
                {
                    cabinetObjectScript.InteractWithThisCabinet();
                }
                else
                {
                    RequestCabinetInteractionServerRpc(cabinetObjectScript.GetComponent<NetworkObject>().NetworkObjectId);
                }
            }
        }
    }

    [ServerRpc]
    private void RequestDoorInteractionServerRpc(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
        {
            doorScript doorScriptObject = networkObject.GetComponent<doorScript>();
            if (doorScriptObject != null)
            {
                doorScriptObject.InteractWithThisDoor();
            }
        }
    }

    [ServerRpc]
    private void RequestDoorXAxisInteractionServerRpc(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
        {
            doorScriptXAxis doorScriptObjectXAxis = networkObject.GetComponent<doorScriptXAxis>();
            if (doorScriptObjectXAxis != null)
            {
                doorScriptObjectXAxis.InteractWithThisDoor();
            }
        }
    }

    [ServerRpc]
    private void RequestFaucetInteractionServerRpc(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
        {
            faucetHandleScript faucetObject = networkObject.GetComponent<faucetHandleScript>();

            if (faucetObject != null)
                faucetObject.InteractWithThisFaucet();
        }
    }

    [ServerRpc]
    private void RequestCabinetInteractionServerRpc(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
        {
            cabinetScript cabinetScriptObject = networkObject.GetComponent<cabinetScript>();
            if (cabinetScriptObject != null)
            {
                cabinetScriptObject.InteractWithThisCabinet();
            }
        }
    }
}
