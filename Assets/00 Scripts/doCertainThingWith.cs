using System;
using System.Collections;
using System.Collections.Generic;
using Obi;
using Tripolygon.UModelerX.Runtime;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class doCertainThingWith : NetworkBehaviour
{   
    const float TONG_GRAB_DISTANCE = 3f;
    const float PIPETTE_GRAB_DISTANCE = 0.3f;
    const float IRON_RING_SNAP_DISTANCE = 0.5f;




    public GameObject itemHeldByTongs; int itemHeldByTongsLayer;
    
    public GameObject heldPipette; public float pipetteSpeed;
    public GameObject closestIronStand; public GameObject closestIronRing;
    
    private bool flowLock = false;

    public pickUpObjects pickUpScript;
    public Vector3 testingOffset;

    // Flag to ensure the pickUpScript is initialized on the client
    private bool isPickUpScriptInitialized = false;


    // Store the NetworkObjectId instead of the reference
    private ulong pickUpScriptNetworkObjectId;

    void Start()
    {
        // Ensure pickUpScript is initialized on the server
        if (IsServer)
        {
            pickUpScript = GetComponent<pickUpObjects>();

            // Store the NetworkObjectId of the pickUpScript object
            pickUpScriptNetworkObjectId = pickUpScript.GetComponent<NetworkObject>().NetworkObjectId;

            // Log to confirm pickUpScript assignment
            Debug.Log("pickUpScript assigned on server.");

            // Sync the pickUpScript network object ID with clients
            SyncPickUpScriptOnClientsClientRpc(pickUpScriptNetworkObjectId);
        }
    }

    // This ClientRpc synchronizes the pickUpScript object ID across clients
    [ClientRpc]
    void SyncPickUpScriptOnClientsClientRpc(ulong networkObjectId)
    {
        if (IsClient)
        {
            // Get the GameObject based on NetworkObjectId
            NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];

            if (networkObject != null)
            {
                // Assign the pickUpScript on the client
                pickUpScript = networkObject.GetComponent<pickUpObjects>();
                Debug.Log("pickUpScript synchronized on client.");
            }
            else
            {
                Debug.LogError("Failed to find pickUpScript object on the client.");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {   
        if (!IsOwner)
            return;
            
        checkForInput();

        if (itemHeldByTongs)
            handleTongObject();

        if (heldPipette){
            lightUpBeaker();
            pipetteSpeed = heldPipette.GetComponent<pipetteScript>().flowSpeed;
        }

        if (pickUpScript.other && pickUpScript.other.name == "Iron Ring") // Snap ring to stand
            checkForIronStandNearby(pickUpScript.other);

        if (pickUpScript.other && pickUpScript.other.name == "Iron Mesh") // Snap mesh to ring
            checkForIronRingNearby(pickUpScript.other);
    }



    void checkForInput(){
        if (Input.GetMouseButtonDown(1))
            findObjectAndPerformAction();

        if (Input.GetMouseButton(1))
            findObjectAndPerformHeldAction();

        if (Input.GetMouseButtonUp(1)){
            findObjectAndPerformLiftedMouseAction();
        }

        checkForKeyOrScrollInputs();
    }

    void findObjectAndPerformAction()       // Right click once
    { 
        if (pickUpScript.other != null){
            GameObject obj = pickUpScript.other;

            if (obj.name == "Fire extinguisher")
                ShootFoam();

            if (obj.name == "Tongs")
                GrabFlaskByNeck(obj);

            if (obj.name == "Matchbox")
                LightMatchAndTossForward(obj);
            
            if (obj.name == "Iron Ring")
                SnapIronRingToStand();
            
            if (obj.name == "Iron Mesh")
                SnapIronMeshToRing();
            
            if (obj.name == "Bunsen Burner")
                faceItemAwayFromPlayer();

        }
    }

    void findObjectAndPerformHeldAction()  // Held right click
    {
        if (pickUpScript.other != null)
        {
            GameObject obj = pickUpScript.other;

            if (obj.name == "Pipette"){
                if (!heldPipette) {
                    heldPipette = obj;
                    pipetteSpeed = heldPipette.GetComponent<pipetteScript>().flowSpeed;
                }
                if (flowLock == false)
                {
                    if (heldPipette.GetComponent<pipetteScript>().pipetteVolume > 0) //is the pipette flowing?
                    {
                        heldPipette.GetComponent<pipetteScript>().pipetteFlowing = true;
                    }
                    else
                    {
                        heldPipette.GetComponent<pipetteScript>().pipetteExtracting = true;
                    }
                    flowLock = true;
                }
                SetPippetteSpeed(obj, pipetteSpeed);
            }

            if (obj.name == "Beaker")
                BringObjectCloser(-1.5f);
            
            if (obj.name == "Evaporating Dish")
                BringObjectCloser(-1.5f);
            
            if (obj.name.StartsWith("Erlenmeyer Flask"))
                BringObjectCloser(-1.5f);

            if (obj.name == "Bunsen Burner")
                manipulateBunsenBurner();

        }
    }

    
    void findObjectAndPerformLiftedMouseAction()  // Lifted Right Click
    {  
        
        if (pickUpScript.other != null) {
            GameObject obj = pickUpScript.other;

            if (obj.name == "Pipette"){
                SetPippetteSpeed(obj, 0f);
                heldPipette.GetComponent<pipetteScript>().pipetteFlowing = false;
                heldPipette.GetComponent<pipetteScript>().pipetteExtracting = false;
                flowLock = false;
            }

            if (obj.name == "Bunsen Burner")
                resetRotationOffset();
        }
    }

    void checkForKeyOrScrollInputs(){
        if (pickUpScript.other != null) {
            GameObject obj = pickUpScript.other;

            if (obj.name == "Bunsen Burner")
                obj.GetComponent<bunsenBurnerScript>().adjustGearBasedOnInput(Input.mouseScrollDelta.y * 2f);

        }

    }

    void BringObjectCloser(float dist)
    {   
        pickUpScript.distOffset = dist;
    }



    void GrabFlaskByNeck(GameObject tongs){

        // Find Closest Flask in the room
        float minDist = Mathf.Infinity;
        GameObject closestFlask = null;
        
        foreach (GameObject currentObject in FindObjectsOfType<GameObject>()){
            
            if (currentObject.name == "Erlenmeyer Flask" || currentObject.name == "Erlenmeyer Flask L"){

                float distFromTip = Vector3.Distance(tongs.transform.Find("Tip").transform.position, currentObject.transform.position);
                
                if (distFromTip < minDist){
                    minDist = distFromTip;
                    closestFlask = currentObject;
                }
            }
        }
        
        // If we have a flask held, drop it
        if (itemHeldByTongs){
            tongs.transform.Find("Open").gameObject.SetActive(true);
            tongs.transform.Find("Closed").gameObject.SetActive(false);
            dropItemFromTongsCorrectly();
            return;
        }
        
        if (!closestFlask || minDist > TONG_GRAB_DISTANCE) // If we cannot pick up flask make sure meshes are good
        {
            itemHeldByTongs = null;
            tongs.transform.Find("Open").gameObject.SetActive(true);
            tongs.transform.Find("Closed").gameObject.SetActive(false);
            return;
        }

        if (closestFlask && minDist <= TONG_GRAB_DISTANCE) // Now we have closest Flask
        {
            tongs.transform.Find("Closed").gameObject.SetActive(true); // Turn on closed mesh
            tongs.transform.Find("Open").gameObject.SetActive(false); // Turn off open mesh
            itemHeldByTongs = closestFlask;
            itemHeldByTongs.GetComponent<Rigidbody>().isKinematic = true;
            itemHeldByTongs.GetComponent<Rigidbody>().useGravity = true;
            itemHeldByTongsLayer = itemHeldByTongs.layer;
            itemHeldByTongs.layer = LayerMask.NameToLayer("HeldByOther");
            var rot = itemHeldByTongs.transform.localEulerAngles;
            itemHeldByTongs.transform.localEulerAngles = new Vector3(0f, rot.y, 0f);
        }
    }

    void handleTongObject(){    // Here Tongs are pos.other
        
        Vector3 offset = Vector3.zero;

        if (itemHeldByTongs.name == "Erlenmeyer Flask")
            offset = pickUpScript.other.transform.TransformDirection(0f,-0.361f,0.1056f);
            
        if (itemHeldByTongs.name == "Erlenmeyer Flask L")
            offset = pickUpScript.other.transform.TransformDirection(0f,-0.452f,0.1056f);



        itemHeldByTongs.transform.position = pickUpScript.other.transform.Find("Tip").position + offset;
    }

    public void dropItemFromTongsCorrectly(){
        if (itemHeldByTongs){
            itemHeldByTongs.GetComponent<Rigidbody>().isKinematic = false;
            itemHeldByTongs.GetComponent<Rigidbody>().useGravity = true;
            itemHeldByTongs.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            itemHeldByTongs.layer = itemHeldByTongsLayer;
        }
    
        pickUpScript.other.transform.Find("Open").gameObject.SetActive(true);
        pickUpScript.other.transform.Find("Closed").gameObject.SetActive(false);
        itemHeldByTongs = null;
    }

    public void dropIronRingCorrectly(){
        pickUpScript.other.transform.Find("Screw").gameObject.SetActive(true); 
        pickUpScript.other.transform.Find("Ring").gameObject.SetActive(true);
        pickUpScript.other.transform.Find("Ghost").gameObject.SetActive(false);
        pickUpScript.other.GetComponent<BoxCollider>().enabled = true;
        pickUpScript.other.tag = "IronRing";
    }

    
    public void dropIronMeshCorrectly(){
        pickUpScript.other.transform.Find("Real").gameObject.SetActive(true);
        pickUpScript.other.transform.Find("Ghost").gameObject.SetActive(false);
        pickUpScript.other.GetComponent<BoxCollider>().enabled = true;
        pickUpScript.other.tag = "Untagged";
    }


    public void SetPippetteSpeed(GameObject pipette, float speed){

        // First find the closest beaker/flask below you
        GameObject closestBeakerOrFlask = findClosestBeakerOrFlask(pipette);
        var pipetteTip = pipette.transform.Find("Tip").transform.position; pipetteTip.y = 0f;
        var beakerOrFlask = closestBeakerOrFlask.transform.position;              beakerOrFlask.y = 0f;

        float distFromTip = Vector3.Distance(pipetteTip, beakerOrFlask);

        if (closestBeakerOrFlask && distFromTip <= PIPETTE_GRAB_DISTANCE){ // We have a beaker or flask within range
            
            pipette.transform.Find("Tip").GetComponent<ObiEmitter>().speed = 0f;
            
            // Add or subtract liquid from beaker based on volume within pipette
            if (closestBeakerOrFlask.transform.Find("Liquid"))
            {
                pipetteScript PS = heldPipette.GetComponent<pipetteScript>();
                liquidScript LS = closestBeakerOrFlask.GetComponent<liquidScript>();
                float amountToAddOrExtract = 50f * Time.deltaTime;

                if (PS.pipetteFlowing) // stop adding liquid if the pipette runs out
                {
                    //adds liquid to the beaker and extracts from pipette
                    float pipetteAmountAfterAdding = PS.pipetteVolume - amountToAddOrExtract;
                    if (pipetteAmountAfterAdding > 0)  //makes sure that the pipette does not give more than it has
                    {
                        //transfers liquid from the pipette to the beaker
                        LS.currentVolume_mL += amountToAddOrExtract;
                        PS.pipetteVolume -= amountToAddOrExtract;
                        LS.addSolution(PS.pipetteSolution, amountToAddOrExtract);
                        LS.updatePercentages();
                        closestBeakerOrFlask.GetComponent<Rigidbody>().AddForce(Vector3.up * 0.0001f, ForceMode.Impulse);
                    }
                    else
                    {
                        //transfers remaining liquid from pipette to beaker
                        LS.currentVolume_mL += PS.pipetteVolume;
                        LS.addSolution(PS.pipetteSolution, PS.pipetteVolume);
                        LS.updatePercentages();
                        PS.pipetteVolume = 0f;
                        closestBeakerOrFlask.GetComponent<Rigidbody>().AddForce(Vector3.up * 0.0001f, ForceMode.Impulse);
                    }
                }
                else if (PS.pipetteExtracting)
                {
                    //Sets the liquid type in the pipette to the liquid type in the beaker (the liquid type will not change inside the pipette)
                    PS.pipetteSolution = LS.solutionMakeup;

                    //Extracts liquid from the beaker into the pipette
                    float beakerAmountAfterExtracting = LS.currentVolume_mL - amountToAddOrExtract;
                    if (beakerAmountAfterExtracting > 0f && PS.pipetteMaxVolume > PS.pipetteVolume + amountToAddOrExtract) //checks if the beaker has liquid and the pipette has room
                    {
                        //transfers liquid from the beaker to the pipette
                        LS.currentVolume_mL -= amountToAddOrExtract;
                        PS.pipetteVolume += 50f * Time.deltaTime;
                        closestBeakerOrFlask.GetComponent<Rigidbody>().AddForce(Vector3.up * 0.0001f, ForceMode.Impulse);
                    }
                    else
                    {
                        // transfers remaining liquid from the beaker to the pipette
                        float amountToFillPipette = PS.pipetteMaxVolume - PS.pipetteVolume;
                        if (LS.currentVolume_mL > (amountToFillPipette)) // checks to see what the limiting factor is: the beaker or the pipette
                        {
                            LS.currentVolume_mL -= amountToFillPipette;
                            PS.pipetteVolume += amountToFillPipette;
                            closestBeakerOrFlask.GetComponent<Rigidbody>().AddForce(Vector3.up * 0.0001f, ForceMode.Impulse);
                        }
                    }
                }
            }
        }

        // We arent within range of a liquid holder
        else{
            pipette.transform.Find("Tip").GetComponent<ObiEmitter>().speed = speed;
            heldPipette.GetComponent<pipetteScript>().pipetteVolume -= 50 * Time.deltaTime;
        }
    }
    
    void lightUpBeaker()
    {
        GameObject closestBeakerOrFlask = findClosestBeakerOrFlask(heldPipette);
        if (closestBeakerOrFlask == null) return;
    
        // Get positions and zero out Y-axis
        Vector3 pipetteTip = heldPipette.transform.Find("Tip").position;
        pipetteTip.y = 0f;
        Vector3 beakerOrFlask = closestBeakerOrFlask.transform.position;
        beakerOrFlask.y = 0f;
    
        float distFromTip = Vector3.Distance(pipetteTip, beakerOrFlask);
    
        // Find "allLiquidHolders" object
        GameObject allLiquidHolders = GameObject.Find("allLiquidHolders");
        if (allLiquidHolders == null) return; // Avoid errors if it's missing
    
        foreach (Transform liquidHolder in allLiquidHolders.transform)
        {
            if (liquidHolder.childCount > 0) // Ensure it has children
            {
                GameObject whiteOutline = liquidHolder.GetChild(0).gameObject;
                bool isClosest = liquidHolder.gameObject == closestBeakerOrFlask && distFromTip <= PIPETTE_GRAB_DISTANCE;
                whiteOutline.SetActive(isClosest);
            }
        }
    }


    GameObject findClosestBeakerOrFlask(GameObject pipette){
        float minDist = Mathf.Infinity;
        GameObject closestBeakerOrFlask = null;
        
        foreach (GameObject currentObject in FindObjectsOfType<GameObject>()){
            
            if (currentObject.tag == "LiquidHolder"){

                var pipetteTip = pipette.transform.Find("Tip").transform.position; pipetteTip.y = 0f;
                var beakerOrFlask = currentObject.transform.position;              beakerOrFlask.y = 0f;

                float distFromTip = Vector3.Distance(pipetteTip, beakerOrFlask);
                
                if (distFromTip < minDist){
                    minDist = distFromTip;
                    closestBeakerOrFlask = currentObject;
                }
            }
        }
        return closestBeakerOrFlask;
    }

    void checkForIronStandNearby(NetworkObjectReference ironRingReference)
    {
        if (IsServer)
        {
            if (ironRingReference.TryGet(out NetworkObject ironRingObject))
            {
                GameObject ironRing = ironRingObject.gameObject;
                if (ironRing == null) return;

                float minDist = float.MaxValue;
                closestIronStand = null;

                foreach (GameObject currentObject in GameObject.FindGameObjectsWithTag("IronStand"))
                {
                    var ironRingPos = ironRing.transform.position; ironRingPos.y = 0f;
                    var ironStandPos = currentObject.transform.position; ironStandPos.y = 0f;

                    float distFromRing = Vector3.Distance(ironRingPos, ironStandPos);

                    if (distFromRing < minDist)
                    {
                        minDist = distFromRing;
                        closestIronStand = currentObject;
                    }
                }

                float yDist = Vector3.Distance(ironRing.transform.Find("Pivot").position, closestIronStand.transform.Find("Base").position);

                if (!closestIronStand || minDist > IRON_RING_SNAP_DISTANCE || yDist > 1.35f)
                {
                    closestIronStand = null;
                    DeactivateGhostRingClientRpc(ironRing);
                    ironRing.GetComponent<BoxCollider>().enabled = true;
                    pickUpScript.other.tag = "IronRing";
                }

                if (closestIronStand && minDist <= IRON_RING_SNAP_DISTANCE && yDist < 1.35f)
                {
                    ActivateGhostRingClientRpc(ironRing);
                    ironRing.GetComponent<BoxCollider>().enabled = false;
                    pickUpScript.other.tag = "NoShadow";

                    var ghostRestingPoint = closestIronStand.transform.Find("Base").position;
                    ghostRestingPoint += closestIronStand.transform.up * yDist;

                    ironRing.transform.Find("Ghost").gameObject.transform.position = ghostRestingPoint;

                    SnapIronStandToClientsClientRpc(ghostRestingPoint);  // ClientRpc to update other clients
                }
            }
            else
            {
                Debug.LogError("Invalid NetworkObjectReference!");
            }
        }
        else
        {
            // Client sends request to server to validate and process this
            RequestIronStandSnapServerRpc(ironRingReference);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestIronStandSnapServerRpc(NetworkObjectReference ironRingReference)
    {
        if (ironRingReference.TryGet(out NetworkObject ironRingObject))
        {
            GameObject ironRing = ironRingObject.gameObject;
            if (ironRing == null) return;

            float minDist = float.MaxValue;
            closestIronStand = null;

            foreach (GameObject currentObject in GameObject.FindGameObjectsWithTag("IronStand"))
            {
                var ironRingPos = ironRing.transform.position;
                ironRingPos.y = 0f;
                var ironStandPos = currentObject.transform.position;
                ironStandPos.y = 0f;

                float distFromRing = Vector3.Distance(ironRingPos, ironStandPos);

                if (distFromRing < minDist)
                {
                    minDist = distFromRing;
                    closestIronStand = currentObject;
                }
            }

            // Check the distance for snapping
            float yDist = Vector3.Distance(ironRing.transform.Find("Pivot").position, closestIronStand.transform.Find("Base").position);

            if (!closestIronStand || minDist > IRON_RING_SNAP_DISTANCE || yDist > 1.35f)
            {
                closestIronStand = null;
                DeactivateGhostRingClientRpc(ironRing);
                ironRing.GetComponent<BoxCollider>().enabled = true;
                pickUpScript.other.tag = "IronRing";
            }

            if (closestIronStand && minDist <= IRON_RING_SNAP_DISTANCE && yDist < 1.35f)
            {
                // Sync with clients
                UpdateClosestIronStandClientRpc(closestIronStand);
                ActivateGhostRingClientRpc(ironRing);
                ironRing.GetComponent<BoxCollider>().enabled = false;
                pickUpScript.other.tag = "NoShadow";

                var ghostRestingPoint = closestIronStand.transform.Find("Base").position;
                ghostRestingPoint += closestIronStand.transform.up * yDist;

                ironRing.transform.Find("Ghost").gameObject.transform.position = ghostRestingPoint;

                SnapIronStandToClientsClientRpc(ghostRestingPoint);  // Update other clients
            }
        }
        else
        {
            Debug.LogError("Invalid NetworkObjectReference!");
        }
    }

    // This ClientRpc updates the closest iron stand reference on the client
    [ClientRpc]
    void UpdateClosestIronStandClientRpc(NetworkObjectReference ironStandReference)
    {
        if (ironStandReference.TryGet(out NetworkObject ironStandObject))
        {
            closestIronStand = ironStandObject.gameObject;  // Update the closest iron stand reference on clients
        }
        else
        {
            Debug.LogError("Failed to resolve iron stand from NetworkObjectReference");
        }
    }

    [ClientRpc]
    void SnapIronStandToClientsClientRpc(Vector3 position)
    {
        // Ensure the pickUpScript and the object it references are not null
        if (pickUpScript != null)
        {
            // Debugging check to see if pickUpScript.other is null
            if (pickUpScript.heldItem != null)
            {
                Transform ghostTransform = pickUpScript.heldItemTransform.Find("Ghost");

                // Check if the "Ghost" object exists before modifying its position
                if (ghostTransform != null)
                {
                    ghostTransform.position = position; // Set the Ghost's position to the desired position
                }
                else
                {
                    Debug.LogError("Ghost object not found in pickUpScript.other!");
                }
            }
            else
            {
                Debug.LogError("pickUpScript.other is null.");
            }
        }
        else
        {
            Debug.LogError("pickUpScript is null.");
        }
    }

    void SnapIronRingToStand()
    {
        GameObject ironRing = pickUpScript.other;

        // Ensure that closestIronStand is assigned before proceeding
        if (closestIronStand == null)
        {
            Debug.LogError("Error: closestIronStand is null! Make sure it is assigned before calling SnapIronRingToStand.");
            return; // Prevent further execution if closestIronStand is null
        }

        // Check if the Ghost is active
        if (ironRing.transform.Find("Ghost").gameObject.activeInHierarchy)
        {
            // Calculate real mesh offset for the positions
            Vector3 realMeshOffset = ironRing.transform.Find("Ghost").Find("Ghost Screw").position - ironRing.transform.Find("Screw").position;

            ironRing.transform.Find("Screw").position += realMeshOffset;
            ironRing.transform.Find("Ring").position += realMeshOffset;
            ironRing.GetComponent<BoxCollider>().center += realMeshOffset;

            // Deactivate Ghost ring and reset the collider
            DeactivateGhostRingClientRpc(ironRing);
            ironRing.GetComponent<BoxCollider>().enabled = true;

            // Set the Rigidbody to kinematic
            ironRing.GetComponent<Rigidbody>().isKinematic = true;

            // Reparent the ironRing on the server and sync it to all clients
            ReparentIronRingServerRpc(ironRing.GetComponent<NetworkObject>(), closestIronStand.GetComponent<NetworkObject>());

            // Drop the item and change the tag back
            GameObject temp = pickUpScript.other;
            pickUpScript.DropItem();
            temp.tag = "IronRing";

            // Sync the new position to all clients
            SnapIronRingToClientsClientRpc(ironRing.transform.position);
        }
    }

    // ServerRpc to reparent the ironRing on the server
    [ServerRpc(RequireOwnership = false)]
    void ReparentIronRingServerRpc(NetworkObjectReference ironRingNetObjRef, NetworkObjectReference ironStandNetObjRef)
    {
        if (ironRingNetObjRef.TryGet(out NetworkObject ironRingObj) && ironStandNetObjRef.TryGet(out NetworkObject ironStandObj))
        {
            // Reparent the ironRing on the server
            ironRingObj.transform.SetParent(ironStandObj.transform);

            SyncIronRingPositionClientRpc(ironRingObj.transform.position, ironStandNetObjRef);
        }
    }

    // ClientRpc to sync the ironRing position and parent change to all clients
    [ClientRpc]
    void SyncIronRingPositionClientRpc(Vector3 position, NetworkObjectReference newParentReference)
    {
        // Attempt to get the NetworkObject for the new parent (iron stand)
        if (pickUpScript.other != null && newParentReference.TryGet(out NetworkObject newParentObject))
        {
            // Update the position of the iron ring
            pickUpScript.other.transform.position = position;

            // Reparent the iron ring to the new parent object (iron stand)
            pickUpScript.other.transform.SetParent(newParentObject.transform);
        }
    }

    [ClientRpc]
    void SnapIronRingToClientsClientRpc(Vector3 position)
    {
        // Update the position of all clients
        if (IsClient)
        {
            pickUpScript.other.transform.position = position;
        }
    }

    void checkForIronRingNearby(NetworkObjectReference ironMeshReference)
    {
        if (IsServer)
        {
            if (!ironMeshReference.TryGet(out NetworkObject ironMeshNetworkObj)) return;
            GameObject ironMesh = ironMeshNetworkObj.gameObject;

            float minDist = float.MaxValue;
            closestIronRing = null;

            foreach (GameObject currentObject in GameObject.FindGameObjectsWithTag("IronRing"))
            {
                Vector3 ironMeshPos = ironMesh.transform.position;
                Vector3 ironRingPos = currentObject.transform.position;

                float distFromMesh = Vector3.Distance(ironMeshPos, ironRingPos);

                if (distFromMesh < minDist)
                {
                    minDist = distFromMesh;
                    closestIronRing = currentObject;
                }
            }

            if (!closestIronRing || minDist > IRON_RING_SNAP_DISTANCE)
            {
                closestIronRing = null;
                DeactivateGhostMeshClientRpc(ironMesh);
                ironMesh.GetComponent<BoxCollider>().enabled = true;
            }
            else
            {
                ActivateGhostMeshClientRpc(ironMesh);
                ironMesh.GetComponent<BoxCollider>().enabled = false;

                pickUpScript.other.tag = "NoShadow"; // Give no shadow to held ghost item

                Vector3 ghostRestingPoint = closestIronRing.transform.Find("Ring").Find("Center").position;

                ironMesh.transform.Find("Ghost").position = ghostRestingPoint;
                ironMesh.transform.Find("Ghost").localEulerAngles = Vector3.zero;

                UpdateGhostPositionOnClientsClientRpc(ghostRestingPoint);
            }
        }
        else
        {
            // Clients request the server to perform proximity checks
            RequestIronRingSnapServerRpc(ironMeshReference);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    void RequestIronRingSnapServerRpc(NetworkObjectReference ironMeshReference)
    {
        if (ironMeshReference.TryGet(out NetworkObject ironMeshObject))
        {
            GameObject ironMesh = ironMeshObject.gameObject;
            checkForIronRingNearby(ironMesh);
        }
    }


    [ClientRpc]
    void UpdateGhostPositionOnClientsClientRpc(Vector3 position)
    {
        // Sync the ghost's position with all clients
        if (IsClient)
        {
            pickUpScript.heldItemTransform.Find("Ghost").position = position;
        }
    }

    void SnapIronMeshToRing()
    {
        GameObject ironMesh = pickUpScript.other;

        if (ironMesh.transform.Find("Ghost").gameObject.activeInHierarchy)
        {
            DeactivateGhostMeshClientRpc(ironMesh);
            ironMesh.GetComponent<BoxCollider>().enabled = true;

            ironMesh.transform.SetParent(closestIronRing.transform);
            ironMesh.transform.localPosition = closestIronRing.transform.Find("Ring").localPosition + closestIronRing.transform.Find("Ring").Find("Center").localPosition;

            var angles = ironMesh.transform.localEulerAngles; angles.x = 0f; angles.z = 0f;
            ironMesh.transform.localEulerAngles = angles;

            ironMesh.GetComponent<Rigidbody>().isKinematic = true;

            pickUpScript.DropItem();

            SnapIronMeshToClientsClientRpc(ironMesh.transform.position);  // Sync with all clients
        }
    }

    [ClientRpc]
    void SnapIronMeshToClientsClientRpc(Vector3 position)
    {
        // Update all clients with the snapped position
        if (IsClient)
        {
            pickUpScript.heldItemTransform.position = position;
        }
    }

    [ClientRpc]
    void ActivateGhostRingClientRpc(NetworkObjectReference ironRingReference)
    {
        if (ironRingReference.TryGet(out NetworkObject ironRingObject))
        {
            GameObject ironRing = ironRingObject.gameObject;
            ironRing.transform.Find("Ghost").gameObject.SetActive(true);
            ironRing.transform.Find("Screw").gameObject.SetActive(false);
            ironRing.transform.Find("Ring").gameObject.SetActive(false);
            ironRing.GetComponent<BoxCollider>().enabled = false;
        }
    }

    [ClientRpc]
    void DeactivateGhostRingClientRpc(NetworkObjectReference ironRingReference)
    {
        if (ironRingReference.TryGet(out NetworkObject ironRingObject))
        {
            GameObject ironRing = ironRingObject.gameObject;
            ironRing.transform.Find("Ghost").gameObject.SetActive(false);
            ironRing.transform.Find("Screw").gameObject.SetActive(true);
            ironRing.transform.Find("Ring").gameObject.SetActive(true);
        }
    }

    [ClientRpc]
    void ActivateGhostMeshClientRpc(NetworkObjectReference ironMeshReference)
    {
        if (ironMeshReference.TryGet(out NetworkObject ironMeshObject))
        {
            GameObject ironMesh = ironMeshObject.gameObject;

            ironMesh.transform.Find("Real").gameObject.SetActive(false);
            ironMesh.transform.Find("Ghost").gameObject.SetActive(true);

        }
    }

    [ClientRpc]
    void DeactivateGhostMeshClientRpc(NetworkObjectReference ironMeshReference)
    {
        if (ironMeshReference.TryGet(out NetworkObject ironMeshObject))
        {
            GameObject ironMesh = ironMeshObject.gameObject;

            ironMesh.transform.Find("Real").gameObject.SetActive(true);
            ironMesh.transform.Find("Ghost").gameObject.SetActive(false);
        }
    }




    void faceItemAwayFromPlayer(){
        pickUpScript.targetRotation.y = transform.localEulerAngles.y;           // When right clicked, face item away from player
    }

    void manipulateBunsenBurner(){

        
        pickUpScript.targetX = 85f;
        pickUpScript.canZoomIn = false;
        pickUpScript.canRotateItem = false;



    }

    void resetRotationOffset(){
        pickUpScript.targetX = 0f;
        pickUpScript.canZoomIn = true;
        pickUpScript.canRotateItem = true;
    }























    void ShootFoam()
    {
        
        if (pickUpScript.other != null && pickUpScript.other.name == "Fire extinguisher")
        {
            
            ParticleSystem foam = pickUpScript.other.transform.Find("Foam").GetComponent<ParticleSystem>();
            if (!foam.isPlaying)
            {
                
                TriggerFoamServerRpc(pickUpScript.other.GetComponent<NetworkObject>().NetworkObjectId);
            }
        }
    }

    [ServerRpc]
    private void TriggerFoamServerRpc(ulong networkObjectId)
    {
       
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        {
            // Call the ClientRpc to trigger the foam effect for all clients
            TriggerFoamClientRpc(networkObjectId);
        }
    }

    [ClientRpc]
    private void TriggerFoamClientRpc(ulong networkObjectId)
    {
        
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        {
            
            ParticleSystem foam = netObj.transform.Find("Foam").GetComponent<ParticleSystem>();
            foam.Play();

            
            StartCoroutine(OnOrOffForDelay(netObj.transform.Find("Spraying").gameObject, foam.main.duration));
            StartCoroutine(OnOrOffForDelay(netObj.transform.Find("Not Spraying").gameObject, foam.main.duration, false));
        }
    }


    IEnumerator OnOrOffForDelay(GameObject obj, float delayTime, bool initialState = true)
    {
        obj.SetActive(initialState);
        yield return new WaitForSeconds(delayTime);
        obj.SetActive(!initialState);
    }

    void LightMatchAndTossForward(GameObject obj)
    {
        
        matchBoxScript matchScript = obj.GetComponent<matchBoxScript>();

        
        if (!matchScript.animationPlaying)
        {
            
            LightMatchServerRpc(obj.GetComponent<NetworkObject>().NetworkObjectId);
        }
    }

    [ServerRpc]
    void LightMatchServerRpc(ulong matchboxNetworkObjectId)
    {
        
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(matchboxNetworkObjectId, out NetworkObject matchboxNetworkObject))
        {
            // Call the ClientRpc to trigger the Light Match for all clients
            LightMatchClientRpc(matchboxNetworkObjectId);
        }
    }

    [ClientRpc]
    void LightMatchClientRpc(ulong matchboxNetworkObjectId)
    {
        
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(matchboxNetworkObjectId, out NetworkObject matchboxNetworkObject))
        {
            
            matchBoxScript matchScript = matchboxNetworkObject.GetComponent<matchBoxScript>();
            if (matchScript != null && !matchScript.animationPlaying)
            {
                matchScript.LightMatch();
            }
        }
    }
}
