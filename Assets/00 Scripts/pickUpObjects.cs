using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using Obi;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.GraphicsBuffer;

public class pickUpObjects : NetworkBehaviour
{
    public Transform playerCamera;
    public float range = 7f;
    public float holdingDistance = 3f; 
    public float blendingSensitivity = 3f;
    float rotationAmInDegrees = 12f;
    public GameObject other;  int otherObjectLayer;

    public float targetX;
    public float targetZ;
    public Vector3 targetRotation;
    public Vector3 meshOffset;
    public Vector3 targetPositionShift;
    Vector3 targetPosition;
    public Vector3 heldObjPosition;
    public bool checkForCollisions;
    public float distOffset = 0f;
    public float checkRadius = 0.5f;
    public multihandler multiHandlerScript;
    public bool canRotateItem;
    public bool canZoomIn = true;

    public Transform shadowCastPoint;
    public Renderer objRenderer;
    public Vector3 objExtents;
    public Vector3 objShift;
    public GameObject shadowGameobject;
    public DecalProjector shadowProjector;

    
    // Def dont need to touch
    bool holdingItem;
    Quaternion targetQuaternion;
    public float initialHoldingDistance; public float untouchedHoldingDistance; public float initialHeldDistForObject; public float distFromGroundLayer;
    public bool readyToMoveBack;
    float xSens;
    Vector3 prev;
    Vector3 previousRotation;
    Vector3 launchTraj;
    Vector3 launchSpin;
    


    void Start(){
        xSens = GetComponent<playerMovement>().xSens;
        playerCamera = transform.GetChild(0);
        initialHoldingDistance = holdingDistance; untouchedHoldingDistance = initialHoldingDistance; initialHeldDistForObject = untouchedHoldingDistance;
        multiHandlerScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<multihandler>();
        shadowCastPoint = GameObject.Find("Shadow For Held Object").transform;
        canRotateItem = true;
        shadowGameobject = transform.Find("Shadow For Held Object").gameObject;
        shadowProjector = shadowGameobject.GetComponent<DecalProjector>();
    }

    void Update()
    {
        if (IsOwner){
            setTargetPosition();

            checkForInput();
            if (checkForCollisions) PreventWeirdIntersections();
            maintainObject();
            handleObjectShadow();

            if (holdingItem) setHelpTextConstantly();
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
        targetX = 0f;
        targetZ = 0f;
        targetRotation = new Vector3(0f, other.transform.localEulerAngles.y, 0f);
        targetQuaternion = Quaternion.Euler(targetRotation);
        objRenderer = other.GetComponent<Renderer>();
        objExtents = other.GetComponent<Collider>().bounds.extents;

        Renderer renderer = other.GetComponent<Renderer>();
        if (renderer){
            Bounds bounds = renderer.bounds;
            Vector3 extents = bounds.extents; 
            checkRadius = (extents.x + extents.z) / 2f;
        
        } else checkRadius = 0.12f;

    
        if (other.GetComponent<shiftBy>()) {
            meshOffset = other.GetComponent<shiftBy>().GetOffset();  objShift = meshOffset;
            targetPositionShift = other.GetComponent<shiftBy>().GetTargetPosOffset(); 
            if (other.GetComponent<shiftBy>().checkRadiusOverride > 0f) checkRadius = other.GetComponent<shiftBy>().checkRadiusOverride; }
        else {
            meshOffset = Vector3.zero;  objShift = meshOffset;
            targetPositionShift = Vector3.zero; }




        initialHoldingDistance = untouchedHoldingDistance;

        if (other.name == "Tongs")
            SetOwnerToTongs();

        if (other.name == "Pipette"){
            initialHoldingDistance = 1.3f;
            canRotateItem = false;
            GetComponent<doCertainThingWith>().heldPipette = other;
        }

        if (other.name == "Bunsen Burner")
            initialHoldingDistance = 1.8f;

        initialHeldDistForObject = initialHoldingDistance;
        setHelpTextBasedOnObject();
    }


    public void DropItem(){

        if (!other) return;

        holdingItem = false;
        canRotateItem = true;
        other.layer = otherObjectLayer;
        Rigidbody rb = other.GetComponent<Rigidbody>();
        rb.linearVelocity = launchTraj; // Launch it
        rb.angularVelocity = launchSpin; // Spin it
        rb.useGravity = true;
        objRenderer = null;
        meshOffset = Vector3.zero;
        objExtents = Vector3.zero;
        objShift = Vector3.zero;
        targetPositionShift = Vector3.zero;
        checkRadius = 0f;
        initialHoldingDistance = untouchedHoldingDistance;
        
        if (other.name == "Tongs")
            GetComponent<doCertainThingWith>().dropItemFromTongsCorrectly();
        
        if (other.name == "Iron Ring")
            GetComponent<doCertainThingWith>().dropIronRingCorrectly();

        if (other.name == "Iron Mesh")
            GetComponent<doCertainThingWith>().dropIronMeshCorrectly();

        if (other.name == "Pipette")
            other.transform.Find("Tip").GetComponent<ObiEmitter>().speed = 0f;    
        

        other = null;holdingItem = false;
        multiHandlerScript.setHelpText("");
    }

    void setHelpTextBasedOnObject(){
        if (other.name == "Beaker")             multiHandlerScript.setHelpText("Right click to view up close.");
        if (other.name == "Fire extinguisher")  multiHandlerScript.setHelpText("Right click to use."); 
        if (other.name == "Tongs")              multiHandlerScript.setHelpText("Right click to grab a flask.");
        if (other.name == "Erlenmeyer Flask")   multiHandlerScript.setHelpText("250 mL Erlenmeyer flask");
        if (other.name == "Erlenmeyer Flask L") multiHandlerScript.setHelpText("500 mL Erlenmeyer flask");
        if (other.name == "Evaporating Dish")   multiHandlerScript.setHelpText("This is an evaporating dish.");
    }

    void setHelpTextConstantly(){
        if (other.name == "Pipette"){
            pipetteScript ps = other.GetComponent<pipetteScript>();
            bool flowing = ps.flowSpeed > 0f;
            multiHandlerScript.setHelpText($"{ps.pipetteVolume} / {ps.pipetteMaxVolume} mL");
        }

        if (other.name == "Bunsen Burner"){
            if (Input.GetMouseButton(1))
                multiHandlerScript.setHelpText($"Scroll to adjust airflow\n{other.GetComponent<bunsenBurnerScript>().airflow.Value.ToString("F2")}/1");
            else
                multiHandlerScript.setHelpText("Right Click to adjust airflow");
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
        
        
        // Rotation Stuff Please Ignore these lines
        if (canRotateItem) targetRotation.y -= Mathf.Min(1f, Input.mouseScrollDelta.y) * rotationAmInDegrees;
        else if (canZoomIn) initialHoldingDistance += Input.mouseScrollDelta.y / 10f;

        if (!Cursor.visible) targetRotation.y += Input.GetAxis("Mouse X") * xSens * Time.deltaTime;
        targetRotation.y = (targetRotation.y % 360f + 360f) % 360f;
        targetQuaternion = Quaternion.Euler(targetRotation);
        
    }

    void checkForRigidbody()
    {
        Ray forwardRay = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        if (Physics.Raycast(forwardRay, out hit, range))
        {
            GameObject hitObject = hit.collider.gameObject;

            // Prevent pickup if the object has doorScript or cabinetScript
            if (hitObject.GetComponent<doorScript>() || hitObject.GetComponent<cabinetScript>())
            {
                return; // Simply exit the function, preventing pickup
            }

            NetworkRigidbody rb = hitObject.GetComponent<NetworkRigidbody>();
            NetworkObject netObj = hitObject.GetComponent<NetworkObject>();

            if (netObj)
            {
                if (!netObj.IsOwner && IsClient)
                {
                    RequestPickUpServerRpc(netObj.NetworkObjectId);
                    return; // Exit since the request has been made
                }
            }

            if (rb && rb.GetComponent<Rigidbody>().isKinematic) // We are trying to pick up a kinematic object - not normal
            {
                if (hit.collider.gameObject.tag == "IronRing"){
                    detachIronRingFromStand(hit.collider.gameObject);
                    return;
                }
                // Debug.Log(hit.collider.gameObject.name);
            }

            if (rb && !rb.GetComponent<Rigidbody>().isKinematic) // ITEM PICKUP
            {
                PickUpItem(hitObject);
            }
        }
    }


    void setTargetPosition(){
        float actualDist = holdingDistance + distOffset;
        targetPosition = playerCamera.position +  playerCamera.forward * actualDist  + playerCamera.TransformDirection(meshOffset);
        
        if (holdingItem){
            targetRotation = new Vector3(targetX, targetRotation.y, targetZ);
            targetQuaternion = Quaternion.Euler(targetRotation);
        }

        

        distOffset /= 1.05f; // Make approach zero when not in use
    }

    void PreventWeirdIntersections()
    {
        if (holdingItem)
        { 
            Ray ray = new Ray(playerCamera.position, targetPosition - playerCamera.position);
            RaycastHit Prehit;

            // Perform the raycast
            if (Physics.Raycast(ray, out Prehit, range, LayerMask.GetMask("Ground"))){
                distFromGroundLayer = Vector3.Distance(playerCamera.position, Prehit.point);
            }
            readyToMoveBack = !Physics.CheckSphere(targetPosition + other.transform.TransformDirection(targetPositionShift), checkRadius * 1.8f, LayerMask.GetMask("Ground"));
            bool targetPositionBlocked = Physics.CheckSphere(targetPosition + targetPositionShift, checkRadius, LayerMask.GetMask("Ground"));

            if (targetPositionBlocked){
                Ray forwardRay = new Ray(playerCamera.position, playerCamera.forward);
                RaycastHit hit;

                if (Physics.Raycast(forwardRay, out hit, holdingDistance)){
                    float distFromCamera = Vector3.Distance(playerCamera.position, hit.point);
                    // Debug.Log(distFromCamera);
                    holdingDistance -= (holdingDistance - distFromCamera)/2f;
                    initialHoldingDistance = holdingDistance;
                }
            } else {
                if (other.name != "Pipette") initialHoldingDistance = Mathf.MoveTowards(initialHoldingDistance, initialHeldDistForObject, blendingSensitivity * Time.deltaTime);
                if (distFromGroundLayer < holdingDistance) 
                    if (other.name != "Matchbox" && other.name != "Tongs" && other.name != "Glass Plate" && !other.name.StartsWith("Beaker")) { 
                        Debug.Log("If you are seeing this and the item is not behind a wall, go to pickUpObjects > PreventWeirdIntersections() and add this name to the if statement"); holdingDistance = distFromGroundLayer - checkRadius; initialHoldingDistance = holdingDistance; }  // Look the tongs were a problem for some reason
                holdingDistance = Mathf.MoveTowards(holdingDistance, initialHoldingDistance, blendingSensitivity * Time.deltaTime / 3f);
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

            heldObjPosition = other.transform.position;
            
            other.transform.localRotation = Quaternion.Slerp(
                other.transform.localRotation,
                targetQuaternion,
                Time.deltaTime * blendingSensitivity
            );

            if (holdingItem){
                float yDistance = 0f;
                if (objRenderer)    {
                    Bounds bounds = objRenderer.bounds;
                    yDistance = Mathf.Abs(bounds.center.y - other.transform.position.y);
                }
                shadowCastPoint.position = heldObjPosition + Vector3.down * (objExtents.y - yDistance);
            }
            else
                shadowCastPoint.position = targetPosition;

            initialHoldingDistance = Mathf.Clamp(initialHoldingDistance, 0.5f, 3f);



            launchTraj = (other.transform.position - prev) / Time.deltaTime;            
            prev = other.transform.position;

            launchSpin = other.transform.eulerAngles - previousRotation;
            previousRotation = other.transform.eulerAngles;
        }
    }

    void handleObjectShadow(){
        if (holdingItem && other.tag != "NoShadow"){
            if (!shadowGameobject.activeInHierarchy)
                shadowGameobject.SetActive(true);

            RaycastHit hit;
            if (Physics.Raycast(shadowCastPoint.position, Vector3.down, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
            {
                float dist = hit.distance;
                // Debug.Log($"Distance to ground: {hit.distance} units.");
                shadowProjector.pivot = new Vector3(0f, 0f, dist);

                float avgSize = objExtents.x + objExtents.z;

                // NECESSARY
                if (dist < 1f) 
                    shadowProjector.size = new Vector3(avgSize, avgSize, dist);
                else
                    shadowProjector.size = new Vector3(avgSize, avgSize, 1f);
            }   
        }
        else
            shadowGameobject.SetActive(false);
    }
























    
    void OnDrawGizmos()
    {
        // Ensure this Gizmo is drawn only for the owning player
        if (IsOwner && playerCamera != null)
        {
            // Draw the holding position sphere
            Gizmos.color = holdingItem ? Color.green : Color.red;
            if (other)
                Gizmos.DrawSphere(targetPosition + other.transform.TransformDirection(targetPositionShift), checkRadius);
            else
                Gizmos.DrawSphere(targetPosition + targetPositionShift, checkRadius);
        }
    }

    void SetOwnerToTongs()
    {
        other.layer = LayerMask.NameToLayer("HeldObject");
        foreach (GameObject currentObject in FindObjectsOfType<GameObject>())
            if (currentObject.name.StartsWith("Erlenmeyer Flask"))
            {   
                NetworkObject networkObject = currentObject.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    ulong networkObjectId = networkObject.NetworkObjectId;
                    ChangeOwnershipServerRpc(networkObjectId, NetworkManager.Singleton.LocalClientId);
                }
            }
    }

    void detachIronRingFromStand(GameObject ironRing){
        Debug.Log("REMOVE");
        ironRing.GetComponent<Rigidbody>().isKinematic = false;
        ironRing.transform.SetParent(null);
        ironRing.transform.Find("Ring").localPosition = new Vector3(0.0256326012f, 0f, 0.0123416251f);      // Reset offsets to original offsets, dont touch these
        ironRing.transform.Find("Screw").localPosition = new Vector3(-0.161500007f, -0.000211842445f, 0.0122618228f);
        ironRing.GetComponent<BoxCollider>().center = new Vector3(0f, 0f, 0.015f);

        if (ironRing.transform.Find("Iron Mesh")){
            ironRing.transform.Find("Iron Mesh").gameObject.GetComponent<Rigidbody>().isKinematic = false;
            ironRing.transform.Find("Iron Mesh").SetParent(null);
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
