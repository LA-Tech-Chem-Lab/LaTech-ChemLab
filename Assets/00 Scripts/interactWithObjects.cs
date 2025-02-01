using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class interactWithObjects : NetworkBehaviour
{
    public Transform playerCamera;
    public float range = 7f;

    public bool playerHoldingObject;
    pickUpObjects pos;

    void Start(){
        pos = GetComponent<pickUpObjects>();
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
        }
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
                    if (IsServer)
                        weightScaleScript.Tare(); // Call the Tare method directly
                    else
                    {
                        Debug.Log("Client Request Tare Button");
                        GameObject scaleObject = GameObject.Find("scale");
                        RequestTareInteractionServerRpc(scaleObject.GetComponent<NetworkObject>().NetworkObjectId);
                    }
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

    [ServerRpc(RequireOwnership = false)]
    private void RequestTareInteractionServerRpc(ulong networkObjectId)
    {
        Debug.Log("progress checkpoint 1");
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
        {
            Debug.Log("progress checkpoint 2");
            WeightScale weightScaleScript = networkObject.GetComponentInChildren<WeightScale>();
            if (weightScaleScript != null)
            {
                Debug.Log("progress checkpoint 3");
                weightScaleScript.Tare();
            }
        }
    }
}
