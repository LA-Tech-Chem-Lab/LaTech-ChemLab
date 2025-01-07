using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class interactWithObjectsNETWORKING : NetworkBehaviour
{
    public Transform playerCamera;
    public float range = 7f;

    void Update()
    {
        if (IsOwner)
        {
            CheckForInput();
        }
    }

    void CheckForInput()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
        {
            CheckForDoors();
            CheckForCabinets();
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
}
