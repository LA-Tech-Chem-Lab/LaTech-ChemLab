using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.GraphicsBuffer;

public class pickUpObjects : NetworkBehaviour
{
    public Transform playerCamera;
    public float range = 7f;
    public float holdingDistance = 3f; 
    public float blendingSensitivity = 3f;
    float rotationAmInDegrees = 12f;
    public GameObject other;  int otherObjectLayer;

    public Vector3 targetRotation;
    public Vector3 meshOffset;
    Vector3 targetPosition;
    public bool checkForCollisions;
    public float distOffset = 0f;
    public float checkRadius = 0.5f;
    public multihandler multiHandlerScript;
    
    // Def dont need to touch
    bool holdingItem;
    Quaternion targetQuaternion;
    float initialHoldingDistance; float untouchedHoldingDistance;
    float xSens;
    Vector3 prev;
    Vector3 previousRotation;
    Vector3 launchTraj;
    Vector3 launchSpin;



    void Start(){
        xSens = GetComponent<playerMovement>().xSens;
        playerCamera = transform.GetChild(0);
        initialHoldingDistance = holdingDistance; untouchedHoldingDistance = initialHoldingDistance;
        multiHandlerScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<multihandler>();
    }

    void Update()
    {
        if (IsOwner){
            setTargetPosition();

            checkForInput();
            if (checkForCollisions) PreventWeirdIntersections();
            maintainObject();
        }
    }

    void PickUpItem(GameObject otherObject)
    {
        if (!otherObject) return; 

        holdingItem = true;
        other = otherObject;
        
        ChangeOwnershipServerRpc(other.GetComponent<NetworkObject>().NetworkObjectId, NetworkManager.Singleton.LocalClientId);

        otherObjectLayer = other.layer;
        other.layer = LayerMask.NameToLayer("HeldObject");
        other.GetComponent<Rigidbody>().useGravity = false;
        targetRotation = new Vector3(0f, other.transform.localEulerAngles.y, 0f);
        targetQuaternion = Quaternion.Euler(targetRotation);

        Renderer renderer = other.GetComponent<Renderer>();
        if (renderer){
            Bounds bounds = renderer.bounds;
            Vector3 extents = bounds.extents; 
            checkRadius = (extents.x + extents.z) / 2f;
        
        } else checkRadius = 0.2f;

        if (other.GetComponent<shiftBy>())
            meshOffset = other.GetComponent<shiftBy>().GetOffset();
        else
            meshOffset = Vector3.zero;


        if (other.name == "Tongs")
            SetOwnerToTongs();

        if (other.name == "Pipette")
            initialHoldingDistance = 1.3f;

        setHelpTextBasedOnObject();
    }

    void SetOwnerToTongs()
    {
        other.layer = LayerMask.NameToLayer("HeldObject");
        foreach (GameObject currentObject in FindObjectsOfType<GameObject>())
            if (currentObject.name == "Erlenmeyer Flask" || currentObject.name == "Erlenmeyer Flask L")
            {   
                NetworkObject networkObject = currentObject.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    ulong networkObjectId = networkObject.NetworkObjectId;
                    ChangeOwnershipServerRpc(networkObjectId, NetworkManager.Singleton.LocalClientId);
                }
            }
    }

    void DropItem(){

        if (!other) return;

        holdingItem = false;
        other.layer = otherObjectLayer;
        Rigidbody rb = other.GetComponent<Rigidbody>();
        rb.linearVelocity = launchTraj; // Launch it
        rb.angularVelocity = launchSpin; // Spin it
        rb.useGravity = true;
        meshOffset = Vector3.zero;
        
        if (other.name == "Tongs")
            GetComponent<doCertainThingWith>().dropItemFromTongsCorrectly();

        if (other.name == "Pipette")
            initialHoldingDistance = untouchedHoldingDistance;

        other = null;holdingItem = false;
        multiHandlerScript.setHelpText("");
    }

    void setHelpTextBasedOnObject(){
        if (other.name == "Beaker")             multiHandlerScript.setHelpText("Right click to view up close.");
        if (other.name == "Pipette")            multiHandlerScript.setHelpText("This is a pipette. Use on a beaker/flask.");
        if (other.name == "Fire extinguisher")  multiHandlerScript.setHelpText("Right click to use."); 
        if (other.name == "Tongs")              multiHandlerScript.setHelpText("Right click to grab a flask.");
        if (other.name == "Erlenmeyer Flask")   multiHandlerScript.setHelpText("250 mL Erlenmeyer flask");
        if (other.name == "Erlenmeyer Flask L") multiHandlerScript.setHelpText("500 mL Erlenmeyer flask");
        if (other.name == "Evaporating Dish")   multiHandlerScript.setHelpText("This is an evaporating dish.");
    }



    void OnDrawGizmos()
    {
        // Ensure this Gizmo is drawn only for the owning player
        if (IsOwner && playerCamera != null)
        {
            // Draw the holding position sphere
            Gizmos.color = holdingItem ? Color.green : Color.red;
            Gizmos.DrawSphere(targetPosition, checkRadius);

            // Draw the forward ray for picking up objects
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(playerCamera.position, playerCamera.forward * range);
        }
    }

    void checkForInput()
    {
        if (Input.GetMouseButtonDown(0) && !Cursor.visible){
                if (!holdingItem)
                    checkForRigidbody();
                else
                    DropItem();
        }
        
        // Rotation Stuff Please Ignore these 4 lines
        if (!Cursor.visible) targetRotation.y += Input.GetAxis("Mouse X") * xSens * Time.deltaTime;
        targetRotation.y -= Mathf.Min(1f, Input.mouseScrollDelta.y) * rotationAmInDegrees;
        targetRotation.y = (targetRotation.y % 360f + 360f) % 360f;
        targetQuaternion = Quaternion.Euler(targetRotation);
        
    }

    void checkForRigidbody()
    {

        Ray forwardRay = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;
        if (Physics.Raycast(forwardRay, out hit, range))
        {
            NetworkRigidbody rb = hit.collider.GetComponent<NetworkRigidbody>();
            if (!rb)
                return;

            NetworkObject netObj = hit.collider.GetComponent<NetworkObject>();
            if (netObj)
            {
                // Debug.Log($"NetworkObject detected. IsOwner: {netObj.IsOwner}, OwnerClientId: {netObj.OwnerClientId}");

                if (!netObj.IsOwner && IsClient)
                {
                    RequestPickUpServerRpc(netObj.NetworkObjectId);
                    return; // Exit since the request has been made
                }
            }

            if (rb && !rb.GetComponent<Rigidbody>().isKinematic)                //  ITEM PICKUP
            {
                PickUpItem(hit.collider.gameObject);
            }
        }
    }

    void setTargetPosition(){
        float actualDist = holdingDistance + distOffset;
        targetPosition = playerCamera.position +  playerCamera.forward * actualDist  + playerCamera.TransformDirection(meshOffset);
        
        distOffset /= 1.1f; // Make approach zero when not in use
    }

    void PreventWeirdIntersections()
    {
        if (holdingItem)
        {   
            bool targetPositionBlocked = Physics.CheckSphere(targetPosition, checkRadius, LayerMask.GetMask("Ground"));

            if (targetPositionBlocked){
                Ray forwardRay = new Ray(playerCamera.position, playerCamera.forward);
                RaycastHit hit;

                if (Physics.Raycast(forwardRay, out hit, holdingDistance)){
                    float distFromCamera = Vector3.Distance(playerCamera.position, hit.point);
                    // Debug.Log(distFromCamera);
                    holdingDistance -= (holdingDistance - distFromCamera)/4f;
                }
            } else {
                holdingDistance = Mathf.MoveTowards(holdingDistance, initialHoldingDistance, blendingSensitivity * Time.deltaTime);
                setTargetPosition();
            }
        }
        
        
    }

    void maintainObject(){
        if (holdingItem && other){

            other.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            other.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            // Debug.DrawRay(other.transform.position, targetPosition - other.transform.position);

            other.transform.position = Vector3.Slerp(
                other.transform.position,
                targetPosition, 
                Time.deltaTime * blendingSensitivity
            );
            
            other.transform.localRotation = Quaternion.Slerp(
                other.transform.localRotation,
                targetQuaternion,
                Time.deltaTime * blendingSensitivity
            );

            launchTraj = (other.transform.position - prev) / Time.deltaTime;            
            prev = other.transform.position;

            launchSpin = (other.transform.eulerAngles - previousRotation);
            previousRotation = other.transform.eulerAngles;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestPickUpServerRpc(ulong networkObjectId, ServerRpcParams rpcParams = default)
    {
        NetworkObject obj = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];
        if (obj != null)
        {
            // Debug.Log("Changing ownership for object: " + obj.name);

            // Change ownership to the requesting client immediately
            obj.ChangeOwnership(rpcParams.Receive.SenderClientId);

                

            // Log ownership change for debugging
            // Debug.Log("New OwnerClientId: " + obj.OwnerClientId);

            // Notify clients immediately to update their UI
            HandlePickUpClientRpc(networkObjectId);
        }
    }



    [ClientRpc]
    void HandlePickUpClientRpc(ulong networkObjectId)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject obj))
        {

            holdingItem = true;
            other = obj.gameObject;
            if (IsOwner)
            {
                // Ensure offset is recalculated for the new owner
                if (other.GetComponent<shiftBy>())
                    meshOffset = other.GetComponent<shiftBy>().GetOffset();
                else
                    meshOffset = Vector3.zero;

                setHelpTextBasedOnObject();

                
                if (obj.name == "Tongs")
                    SetOwnerToTongs();
            }
            // DropItem();
            // PickUpItem(obj.gameObject);
        }
        else
        {
            // Debug.LogWarning("Object not found on client.");
        }
    }

    
    [ServerRpc(RequireOwnership = false)]
    void ChangeOwnershipServerRpc(ulong networkObjectId, ulong newOwnerClientId, ServerRpcParams rpcParams = default)
    {
        // Get the NetworkObject based on the provided ID
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];

        if (networkObject != null)
        {
            // Check if the current owner is already the requested owner
            if (networkObject.OwnerClientId == newOwnerClientId)  // Debug.Log($"Ownership of object {networkObject.name} is already held by client {newOwnerClientId}, skipping ownership change.");
                return;

            // Change ownership on the server
            networkObject.ChangeOwnership(newOwnerClientId);
        }
    }


}
