using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.GraphicsBuffer;

public class pickUpObjectsNETWORKING : NetworkBehaviour
{
    public Transform playerCamera;
    public float range = 7f;
    public float holdingDistance = 3f; 
    float initialHoldingDistance;
    public float blendingSensitivity = 3f;
    public GameObject other;  int otherObjectLayer;

    bool holdingItem;
    Vector3 prev;
    Vector3 previousRotation;
    Vector3 launchTraj;
    
    Vector3 launchSpin;
    Vector3 targetRotation;
    Vector3 meshOffset;
    Vector3 targetPosition;
    Quaternion targetQuaternion;
    float xSens;
    public bool checkForCollisions;
    public float distOffset = 0f;
    public float checkRadius = 0.5f;
    bool targetPositionBlocked;
    public multihandler multiHandlerScript;

    void Start(){
        xSens = GetComponent<playerMovementNETWORKING>().xSens;
        playerCamera = transform.GetChild(0);
        initialHoldingDistance = holdingDistance;
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

    void PickUpItem(GameObject otherObject){

            holdingItem = true;
            other = otherObject;
            otherObjectLayer = other.layer;
            other.layer = LayerMask.NameToLayer("HeldObject");
            other.GetComponent<Rigidbody>().useGravity = false;
            targetRotation = new Vector3(0f, other.transform.localEulerAngles.y, 0f);
            targetQuaternion = Quaternion.Euler(targetRotation);

            if (other.GetComponent<shiftBy>())
                meshOffset = other.GetComponent<shiftBy>().GetOffset();
            else
                meshOffset = Vector3.zero;

            setHelpTextBasedOnObject();
    }

    void DropItem(){
        holdingItem = false;
        other.layer = otherObjectLayer;
        Rigidbody rb = other.GetComponent<Rigidbody>();
        rb.linearVelocity = launchTraj; // Launch it
        rb.angularVelocity = launchSpin; // Launch it
        rb.useGravity = true;
        other = null;
        
        multiHandlerScript.setHelpText("");
    }

    void setHelpTextBasedOnObject(){
        if (other.name == "Beaker")             multiHandlerScript.setHelpText("Right Click to view up close.");
        if (other.name == "Fire extinguisher")  multiHandlerScript.setHelpText("Right Click to use."); 
    }



    void OnDrawGizmos()
    {
        // Ensure this Gizmo is drawn only for the owning player
        if (IsOwner && playerCamera != null)
        {
            // Draw the holding position sphere
            Gizmos.color = holdingItem ? Color.green : Color.red;
            Gizmos.DrawSphere(targetPosition+Vector3.up*checkRadius, checkRadius);

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
        targetRotation.y -= Mathf.Min(1f, Input.mouseScrollDelta.y) * blendingSensitivity;
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
        
        distOffset /= 1.1f;
    }

    void PreventWeirdIntersections()
    {
        if (holdingItem)
        {   
            targetPositionBlocked = Physics.CheckSphere(targetPosition+Vector3.up*checkRadius, checkRadius, LayerMask.GetMask("Ground"));

            if (targetPositionBlocked){
                Ray forwardRay = new Ray(playerCamera.position, playerCamera.forward);
                RaycastHit hit;

                if (Physics.Raycast(forwardRay, out hit, holdingDistance)){
                    float distFromCamera = Vector3.Distance(playerCamera.position, hit.point);
                    // Debug.Log(distFromCamera);
                    holdingDistance -= (holdingDistance - distFromCamera);
                }
            } else {
                holdingDistance = Mathf.MoveTowards(holdingDistance, initialHoldingDistance, blendingSensitivity * Time.deltaTime);
                setTargetPosition();
            }
        }
        
        
    }

    void maintainObject(){
        if (holdingItem){

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
            // Log the pickup request
            // Debug.Log($"Object {obj.name} picked up by client {rpcParams.Receive.SenderClientId}.");

            // Change ownership to the requesting client
            obj.ChangeOwnership(rpcParams.Receive.SenderClientId);

            // Notify all clients to handle the object as picked up
            HandlePickUpClientRpc(networkObjectId);
        }
        // else
        // {
        //     Debug.LogError($"Object with ID {networkObjectId} not found!");
        // }
    }


    [ClientRpc]
    void HandlePickUpClientRpc(ulong networkObjectId)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject obj))
        {
            // Client-side handling for picking up the object
            holdingItem = true;
            other = obj.gameObject;

            // Optional: Perform any additional logic, e.g., adjust position or rotation
            Debug.Log($"Object {obj.name} is now held by client.");
        }
        else
        {
            Debug.LogWarning("Object not found on client.");
        }
    }

}
