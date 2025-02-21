using System;
using System.Collections;
using System.Collections.Generic;
using Obi;
using Tripolygon.UModelerX.Runtime;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class doCertainThingWith : NetworkBehaviour
{   
    const float TONG_GRAB_DISTANCE = 3f;
    const float PIPETTE_GRAB_DISTANCE = 0.3f;
    const float IRON_RING_SNAP_DISTANCE = 0.7f;
    const float SCOOPULA_GRAB_DISTANCE = 1.2f;
    const float FUNNEL_INSERT_DISTANCE = 1.5f;
    const float ALUMINUM_DROPOFF_RANGE = 0.8f;


    public GameObject itemHeldByTongs; int itemHeldByTongsLayer;
    
    public GameObject heldPipette; public float pipetteSpeed;
    public GameObject closestIronStand; public GameObject closestIronRing;

    public GameObject ironMesh;

    private bool flowLock = false;
    private bool scoopulaAnimationPlaying = false;


    pickUpObjects pickUpScript;
    public Vector3 testingOffset;
    public bool funnelIsAttatched = false;
    public GameObject funneledFlask = null;
    public GameObject filteredFunnel = null;
    public bool filterIsAttatched = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pickUpScript = GetComponent<pickUpObjects>();
        ironMesh = GameObject.Find("Iron Mesh");
        Rigidbody rb = ironMesh.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
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
            if (heldPipette == pickUpScript.other){
                lightUpBeaker();
                pipetteSpeed = heldPipette.GetComponent<pipetteScript>().flowSpeed;
            }
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

            if (obj.name == "Scoopula")
                GatherAluminumPelletsFromContainerOrDropThem();
            
            if (obj.name == "Glass Funnel")
                insertFunnel(obj);

            if (obj.name == "Paper Cone")
                insertFilter(obj);


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
                BringObjectCloser(-1.1f);
            
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
        if (pickUpScript.other != null)
        {
            GameObject obj = pickUpScript.other;

            if (obj.name == "Bunsen Burner")
                if (Input.GetMouseButton(1))
                {
                    obj.GetComponent<bunsenBurnerScript>().AdjustAirflowBasedOnInput(Input.mouseScrollDelta.y * 2f);
                    obj.GetComponent<bunsenBurnerScript>().AdjustGearRotationServerRpc(Input.mouseScrollDelta.y * 2f);
                }
        }
    }

    void BringObjectCloser(float dist)
    {   
        pickUpScript.distOffset = dist;
    }

    void insertFunnel(GameObject funnel) {
        float minDist = Mathf.Infinity;
        GameObject closestFlask = null;
        Transform flaskOpening = null;

        // Find the closest Flask
        foreach (GameObject currentObject in FindObjectsOfType<GameObject>()) {
            if (currentObject.name == "Erlenmeyer Flask" || currentObject.name == "Erlenmeyer Flask L") {
                float distFromFunnel = Vector3.Distance(funnel.transform.position, currentObject.transform.position);

                if (distFromFunnel < minDist) {
                    minDist = distFromFunnel;
                    closestFlask = currentObject;

                    // Find the flask's top position
                    flaskOpening = closestFlask.transform.Find("FlaskTop");
                }
            }
        }

        // Attach the funnel to the flask if within range
        if (closestFlask && flaskOpening && minDist <= FUNNEL_INSERT_DISTANCE) {
            pickUpScript.DropItem();

            // Attach funnel to flask
            funnel.transform.position = flaskOpening.position;
            funnel.transform.rotation = flaskOpening.rotation;

            // Make it a child so it follows movement
            funnel.transform.SetParent(closestFlask.transform);

            // Disable physics and collisions so it stays attached
            Physics.IgnoreCollision(funnel.GetComponent<Collider>(), closestFlask.GetComponent<Collider>(), true);

            Rigidbody rb = funnel.GetComponent<Rigidbody>();
            if (rb) {
                rb.isKinematic = true;
            }
            funneledFlask = closestFlask;
            funneledFlask.tag = "Untagged";
            funnelIsAttatched = true;
        }
    }

    public void DetachFunnel(GameObject funnel) {
        // Remove parent so it no longer follows the flask
        funnel.transform.SetParent(null);

        // Re-enable physics and collisions
        Physics.IgnoreCollision(funnel.GetComponent<Collider>(), funneledFlask.GetComponent<Collider>(), false);

        Rigidbody rb = funnel.GetComponent<Rigidbody>();
        if (rb) {
            rb.isKinematic = false;
        }
        funneledFlask.tag = "LiquidHolder";
        funneledFlask = null;
        funnelIsAttatched = false;
    }

    void insertFilter(GameObject filter) {
        float minDist = Mathf.Infinity;
        GameObject closestFunnel = null;
        Transform funnelOpening = null;

        // Find the closest Flask
        foreach (GameObject currentObject in FindObjectsOfType<GameObject>()) {
            if (currentObject.name == "Glass Funnel" && (currentObject.transform.parent.name == "Erlenmeyer Flask" || currentObject.transform.parent.name == "Erlenmeyer Flask L")) {
                float distFromFilter = Vector3.Distance(filter.transform.position, currentObject.transform.position);

                if (distFromFilter < minDist) {
                    minDist = distFromFilter;
                    closestFunnel = currentObject;

                    // Find the flask's top position
                    funnelOpening = closestFunnel.transform.Find("FunnelTop");
                }
            }
        }

        // Attach the funnel to the flask if within range
        if (closestFunnel && funnelOpening && minDist <= FUNNEL_INSERT_DISTANCE) {
            pickUpScript.DropItem();

            // Attach funnel to flask
            filter.transform.position = funnelOpening.position;
            filter.transform.rotation = funnelOpening.rotation;

            Debug.Log(closestFunnel.name);
            // Make it a child so it follows movement
            filter.transform.SetParent(closestFunnel.transform);

            // Disable physics and collisions so it stays attached
            Physics.IgnoreCollision(filter.GetComponent<Collider>(), closestFunnel.GetComponent<Collider>(), true);

            Rigidbody rb = filter.GetComponent<Rigidbody>();
            if (rb) {
                rb.isKinematic = true;
            }

            filteredFunnel = closestFunnel;
            filterIsAttatched = true;
        }
    }

    public void DetachFilter(GameObject filter) {
        // Remove parent so it no longer follows the flask
        filter.transform.SetParent(null);

        // Re-enable physics and collisions
        Physics.IgnoreCollision(filter.GetComponent<Collider>(), filteredFunnel.GetComponent<Collider>(), false);

        Rigidbody rb = filter.GetComponent<Rigidbody>();
        if (rb) {
            rb.isKinematic = false;
        }
        filteredFunnel = null;
        filterIsAttatched = false;
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
        var pipetteTip = pipette.transform.Find("Tip").transform.position;        pipetteTip.y = 0f;
        var beakerOrFlask = closestBeakerOrFlask.transform.position;              beakerOrFlask.y = 0f;

        float distFromTip = Vector3.Distance(pipetteTip, beakerOrFlask);

        if (closestBeakerOrFlask && distFromTip <= PIPETTE_GRAB_DISTANCE){ // We have a beaker or flask within range
            
            pipette.transform.Find("Tip").GetComponent<ObiEmitter>().speed = 0f;
            
            // Add or subtract liquid from beaker based on volume within pipette
            if (closestBeakerOrFlask.transform.Find("Liquid"))
            {
                pipetteScript PS = heldPipette.GetComponent<pipetteScript>();
                float realFlowRate = PS.flowRateML;
                liquidScript LS = closestBeakerOrFlask.GetComponent<liquidScript>();
                float amountToAddOrExtract = realFlowRate * Time.deltaTime;

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
                    heldPipette.GetComponent<liquidScript>().solutionMakeup = LS.solutionMakeup;

                    //Extracts liquid from the beaker into the pipette
                    float beakerAmountAfterExtracting = LS.currentVolume_mL - amountToAddOrExtract;
                    if (beakerAmountAfterExtracting > 0f && PS.pipetteMaxVolume > PS.pipetteVolume + amountToAddOrExtract) //checks if the beaker has liquid and the pipette has room
                    {
                        //transfers liquid from the beaker to the pipette
                        LS.currentVolume_mL -= amountToAddOrExtract;
                        PS.pipetteVolume += amountToAddOrExtract;
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
            heldPipette.GetComponent<pipetteScript>().pipetteVolume -= Time.deltaTime;
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

    public void turnOffBeakers(){
        GameObject allLiquidHolders = GameObject.Find("allLiquidHolders");
        if (allLiquidHolders == null) return; // Avoid errors if it's missing
    
        foreach (Transform liquidHolder in allLiquidHolders.transform)
        {
            if (liquidHolder.childCount > 0) // Ensure it has children
            {
                GameObject whiteOutline = liquidHolder.GetChild(0).gameObject;
                whiteOutline.SetActive(false);
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

    void checkForIronStandNearby(GameObject ironRing)
    {
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

        // No Go
        if (!closestIronStand || minDist > IRON_RING_SNAP_DISTANCE || yDist > 1.35f)
        {
            closestIronStand = null;
            ironRing.transform.Find("Screw").gameObject.SetActive(true);
            ironRing.transform.Find("Ring").gameObject.SetActive(true);
            ironRing.transform.Find("Ghost").gameObject.SetActive(false);
            ironRing.GetComponent<BoxCollider>().enabled = true;
            pickUpScript.other.tag = "IronRing";
        }

        // Now we have closest stand
        if (closestIronStand && minDist <= IRON_RING_SNAP_DISTANCE && yDist < 1.35f)
        {
            ironRing.transform.Find("Screw").gameObject.SetActive(false);
            ironRing.transform.Find("Ring").gameObject.SetActive(false);
            ironRing.transform.Find("Ghost").gameObject.SetActive(true);
            ironRing.GetComponent<BoxCollider>().enabled = false;
            pickUpScript.other.tag = "NoShadow";

            var ghostRestingPoint = closestIronStand.transform.Find("Base").position;
            ghostRestingPoint += closestIronStand.transform.up * yDist;

            ironRing.transform.Find("Ghost").gameObject.transform.position = ghostRestingPoint;

            // Inform the server about the closest iron stand
            if (IsOwner)
            {
                // Get the NetworkObjectIds for the iron ring and closest iron stand
                ulong ironRingId = pickUpScript.other.GetComponent<NetworkObject>().NetworkObjectId;
                ulong ironStandId = closestIronStand.GetComponent<NetworkObject>().NetworkObjectId;

                // Call the server RPC to update the closest iron stand
                UpdateClosestIronStandServerRpc(ironRingId, ironStandId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateClosestIronStandServerRpc(ulong ironRingId, ulong ironStandId, ServerRpcParams rpcParams = default)
    {
        // Find the iron ring by its NetworkObjectId
        NetworkObject ironRingNetObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[ironRingId];
        if (ironRingNetObj != null)
        {
            // Find the iron stand by its NetworkObjectId
            NetworkObject ironStandNetObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[ironStandId];
            if (ironStandNetObj != null)
            {
                // Now update the closest iron stand on the server
                closestIronStand = ironStandNetObj.gameObject;
            }
        }
    }

    void SnapIronRingToStand()
    {
        GameObject ironRing = pickUpScript.other;

        if (ironRing.transform.Find("Ghost").gameObject.activeInHierarchy) // Ready to snap and right-clicked
        {
            if (IsHost)
            {
                Debug.Log("[SERVER] This is the host, processing iron ring snap.");
            }
            else
            {
                Debug.Log("[CLIENT] This is a client, triggering action on server.");
            }
            Vector3 realMeshOffset = ironRing.transform.Find("Ghost").Find("Ghost Screw").position - ironRing.transform.Find("Screw").position;


            ironRing.transform.Find("Screw").gameObject.SetActive(true);
            ironRing.transform.Find("Ring").gameObject.SetActive(true);
            ironRing.transform.Find("Ghost").gameObject.SetActive(false);
            ironRing.GetComponent<BoxCollider>().enabled = true;

            ironRing.GetComponent<Rigidbody>().isKinematic = true; // Set to kinematic

            GameObject temp = pickUpScript.other;
            pickUpScript.DropItem(); // Probably the only way
            temp.tag = "IronRing";

            // Ensure the Iron Ring has a NetworkObject and is spawned before syncing
            if (ironRing.TryGetComponent<NetworkObject>(out var netObj) && netObj.IsSpawned)
            {
                // If client initiated, notify the server
                if (!IsHost)
                {
                    Debug.Log("Client Trying");
                    // Call server-side function to handle syncing and actions
                    NotifyServerToSnapIronRingServerRpc(netObj.NetworkObjectId, ironRing.transform.position, realMeshOffset, true);
                }
                else
                {
                    Debug.Log("Server Trying");
                    // If host initiates, proceed directly
                    SyncIronRingPositionAndOffsetServerRpc(netObj.NetworkObjectId, ironRing.transform.position, realMeshOffset, true);
                    SetParentServerRpc(netObj.NetworkObjectId, closestIronStand.GetComponent<NetworkObject>().NetworkObjectId);
                }
            }

            // Debug log for host only
            if (IsHost)
            {
                Debug.Log($"[HOST] Iron Ring Position: {ironRing.transform.position}");
            }
        }
    }

    // ServerRpc for client to notify the server to handle the snapping and synchronization
    [ServerRpc(RequireOwnership = false)]
    void NotifyServerToSnapIronRingServerRpc(ulong ironRingId, Vector3 position, Vector3 offset, bool isKinematic, ServerRpcParams rpcParams = default)
    {
        
        // Optionally, trigger SetParentServerRpc for server-side parenting
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ironRingId, out NetworkObject ironRingNetObj))
        {
            SetParentServerRpc(ironRingId, closestIronStand.GetComponent<NetworkObject>().NetworkObjectId);
        }
        SyncIronRingPositionAndOffsetServerRpc(ironRingId, position, offset, isKinematic);
    }



    // ServerRpc for syncing position and offset across all clients
    [ServerRpc(RequireOwnership = false)]
    void SyncIronRingPositionAndOffsetServerRpc(ulong ironRingId, Vector3 position, Vector3 offset, bool isKinematic, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"[SERVER] Iron Ring Snap Triggered on Server. Position: {position}, Offset: {offset}, Is Kinematic: {isKinematic}");

        // Ensure the iron ring position is updated on the server before syncing
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ironRingId, out NetworkObject ironRingNetObj))
        {
            GameObject ironRing = ironRingNetObj.gameObject;

            ironRing.transform.position = position;
            ironRing.transform.Find("Screw").position += offset;
            ironRing.transform.Find("Ring").position += offset;
            ironRing.GetComponent<BoxCollider>().center = ironRing.transform.Find("Ring").localPosition;
            Debug.Log("Local Position of the iron ring child called ring" + ironRing.transform.Find("Ring").localPosition);
            ironRing.GetComponent<Rigidbody>().isKinematic = isKinematic;

            // Sync to clients
            SyncIronRingPositionAndOffsetClientRpc(ironRingId, position, offset, isKinematic);
        }
    }


    // ClientRpc to update clients with the position and offset
    [ClientRpc]
    void SyncIronRingPositionAndOffsetClientRpc(ulong ironRingId, Vector3 position, Vector3 offset, bool isKinematic)
    {
        if (!IsHost) // Only clients update (Host already has the correct position)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ironRingId, out NetworkObject ironRingNetObj))
            {
                GameObject ironRing = ironRingNetObj.gameObject;

                // Apply the position and offset on the client as well
                ironRing.transform.position = position;
                Debug.Log("Offset is " + offset);
                ironRing.transform.Find("Screw").position += offset;
                ironRing.transform.Find("Ring").position += offset;
                ironRing.GetComponent<BoxCollider>().center = ironRing.transform.Find("Ring").localPosition;
                Debug.Log("Local Position of the iron ring child called ring" + ironRing.transform.Find("Ring").localPosition);
                ironRing.GetComponent<Rigidbody>().isKinematic = isKinematic;

                // Debug log for clients only
                Debug.Log($"[CLIENT] Iron Ring Position: {ironRing.transform.position}");
            }
        }
    }

    // ServerRpc for setting the parent on the server
    [ServerRpc(RequireOwnership = false)]
    void SetParentServerRpc(ulong ironRingId, ulong standId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ironRingId, out NetworkObject ironRingNetObj) &&
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(standId, out NetworkObject standNetObj))
        {
            ironRingNetObj.transform.SetParent(standNetObj.transform);
        }
    }



    void checkForIronRingNearby(GameObject ironMesh){

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

        // No Go
        if (!closestIronRing || minDist > IRON_RING_SNAP_DISTANCE){
            
            closestIronRing = null;
            ironMesh.transform.Find("Real").gameObject.SetActive(true);
            ironMesh.transform.Find("Ghost").gameObject.SetActive(false);
            ironMesh.GetComponent<BoxCollider>().enabled = false;
        }
        

        // Now we have closest ring
        if (closestIronRing && minDist <= IRON_RING_SNAP_DISTANCE && closestIronRing.transform.parent != null) {
            ironMesh.transform.Find("Real").gameObject.SetActive(false);
            ironMesh.transform.Find("Ghost").gameObject.SetActive(true);
            ironMesh.GetComponent<BoxCollider>().enabled = true;
            
            pickUpScript.other.tag = "NoShadow"; // Give no shadow to held ghost item

            Vector3 ghostRestingPoint = closestIronRing.transform.Find("Ring").Find("Center").position;

            ironMesh.transform.Find("Ghost").position = ghostRestingPoint;
            ironMesh.transform.Find("Ghost").localEulerAngles = Vector3.zero;
            // Inform the server about the closest iron ring
            if (IsOwner)
            {
                // Get the NetworkObjectIds for the iron ring and closest iron stand
                ulong ironMeshId = pickUpScript.other.GetComponent<NetworkObject>().NetworkObjectId;
                ulong ironRingId = closestIronRing.GetComponent<NetworkObject>().NetworkObjectId;
                Debug.Log("Closest Iron Ring: " + closestIronRing);
                // Call the server RPC to update the closest iron stand
                UpdateClosestIronRingServerRpc(ironMeshId, ironRingId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateClosestIronRingServerRpc(ulong ironMeshId, ulong ironRingId, ServerRpcParams rpcParams = default)
    {
        // Find the iron ring by its NetworkObjectId
        NetworkObject ironMeshNetObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[ironMeshId];
        if (ironMeshNetObj != null)
        {
            // Find the iron stand by its NetworkObjectId
            NetworkObject ironRingNetObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[ironRingId];
            if (ironRingNetObj != null)
            {
                // Now update the closest iron stand on the server
                closestIronRing = ironRingNetObj.gameObject;
                Debug.Log("Closest Iron Ring: " + closestIronRing);
            }
        }
    }

    void SnapIronMeshToRing()
    {
        GameObject ironMesh = pickUpScript.other;


        if (ironMesh.transform.Find("Ghost").gameObject.activeInHierarchy)
        { // We are ready to snap and we right clicked

            ironMesh.transform.Find("Real").gameObject.SetActive(true);
            ironMesh.transform.Find("Ghost").gameObject.SetActive(false);
            ironMesh.GetComponent<BoxCollider>().enabled = true;

            ironMesh.GetComponent<Rigidbody>().isKinematic = true;
            var localPosition = closestIronRing.transform.Find("Ring").localPosition + closestIronRing.transform.Find("Ring").Find("Center").localPosition;
            pickUpScript.DropItem(); // prob the only way

            // Ensure the Iron Mesh has a NetworkObject and is spawned before syncing
            if (ironMesh.TryGetComponent<NetworkObject>(out var netObj) && netObj.IsSpawned)
            {
                // If client initiated, notify the server
                if (!IsHost)
                {
                    Debug.Log("Client Trying Mesh");
                    // Call server-side function to handle syncing and actions
                    NotifyServerToSnapIronMeshServerRpc(netObj.NetworkObjectId, localPosition, true);
                }
                else
                {
                    Debug.Log("Server Trying Mesh ");
                    // If host initiates, proceed directly
                    SetParentMeshServerRpc(netObj.NetworkObjectId, closestIronRing.GetComponent<NetworkObject>().NetworkObjectId);
                    SyncIronMeshPositionServerRpc(netObj.NetworkObjectId, localPosition, true);
                }
            }
            // Debug log for host only
            if (IsHost)
            {
                Debug.Log("Snap Function Debug");
                Debug.Log($"[HOST] Iron Mesh Position: {ironMesh.transform.localPosition}");
            }
        }
    }

    // ServerRpc for client to notify the server to handle the snapping and synchronization
    [ServerRpc(RequireOwnership = false)]
    void NotifyServerToSnapIronMeshServerRpc(ulong ironMeshId, Vector3 position, bool isKinematic, ServerRpcParams rpcParams = default)
    {

        
        // Optionally, trigger SetParentServerRpc for server-side parenting
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ironMeshId, out NetworkObject ironMeshNetObj))
        {

            SetParentMeshServerRpc(ironMeshId, closestIronRing.GetComponent<NetworkObject>().NetworkObjectId);
            
        }

        SyncIronMeshPositionServerRpc(ironMeshId, position, isKinematic);
    }

    // ServerRpc for syncing position and offset across all clients
    [ServerRpc(RequireOwnership = false)]
    void SyncIronMeshPositionServerRpc(ulong ironMeshId, Vector3 position, bool isKinematic, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"[SERVER] Iron Mesh Snap Triggered on Server. Position: {position}, Is Kinematic: {isKinematic}");

        // Ensure the iron ring position is updated on the server before syncing
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ironMeshId, out NetworkObject ironMeshNetObj))
        {
            Debug.Log("Position that it should be: " + position);
            GameObject ironMesh = ironMeshNetObj.gameObject;
            Debug.Log("Current Position Before TRYING" + ironMesh.transform.localPosition);
            Debug.Log("POSITION BEFORE TRYING" + position);
            ironMesh.transform.localPosition = position; 
            Debug.Log("Current Position AFTER TRYING" + ironMesh.transform.localPosition);
            var angles = ironMesh.transform.localEulerAngles; angles.x = 0f; angles.z = 0f;
            ironMesh.transform.localEulerAngles = angles;
            ironMesh.GetComponent<Rigidbody>().isKinematic = isKinematic;
            Debug.Log($"[SERVER] End Results, Mesh {ironMesh.transform.localPosition}");
            // Sync to clients
            SyncIronMeshPositionClientRpc(ironMeshId, position, isKinematic);
        }
    }

    // ClientRpc to update clients with the position and offset
    [ClientRpc]
    void SyncIronMeshPositionClientRpc(ulong ironMeshId, Vector3 position, bool isKinematic)
    {
        if (!IsHost) // Only clients update (Host already has the correct position)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ironMeshId, out NetworkObject ironMeshNetObj))
            {
                GameObject ironMesh = ironMeshNetObj.gameObject;
                ironMesh.transform.localPosition = position;
                var angles = ironMesh.transform.localEulerAngles; angles.x = 0f; angles.z = 0f;
                ironMesh.transform.localEulerAngles = angles;
                ironMesh.GetComponent<Rigidbody>().isKinematic = isKinematic;


                // Debug log for clients only
                Debug.Log($"[CLIENT] Iron Ring Position: {ironMesh.transform.localPosition}");
            }
        }
    }

    // ServerRpc for setting the parent on the server
    [ServerRpc(RequireOwnership = false)]
    void SetParentMeshServerRpc(ulong ironMeshId, ulong RingId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ironMeshId, out NetworkObject ironMeshNetObj) &&
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(RingId, out NetworkObject RingNetObj))
        {
            ironMeshNetObj.transform.SetParent(RingNetObj.transform);
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


    void GatherAluminumPelletsFromContainerOrDropThem(){
        GameObject scoopula = pickUpScript.other;

        // Find Closest Aluminum Container in the room
        float minDist = Mathf.Infinity;
        GameObject closestAluminumContainer = null;
        
        foreach (GameObject currentObject in FindObjectsOfType<GameObject>()){
            
            if (currentObject.name == "Aluminum Container"){

                float distFromTip = Vector3.Distance(scoopula.transform.position, currentObject.transform.position);
                
                if (distFromTip < minDist){
                    minDist = distFromTip;
                    closestAluminumContainer = currentObject;
                }
            }
        }
        
        

        if (closestAluminumContainer && minDist <= SCOOPULA_GRAB_DISTANCE) // Now we have closest Flask
        {
            if (!scoopulaAnimationPlaying){
                StartCoroutine(getAluminumUsingScoopula(closestAluminumContainer));
                return;
            }
        }

        // Okay, fine lets try to drop it instead then
        GameObject closestBeakerOrFlask = findClosestBeakerOrFlask(scoopula);
        var pipetteTip = scoopula.transform.Find("Tip").transform.position;        pipetteTip.y = 0f;
        var beakerOrFlask = closestBeakerOrFlask.transform.position;              beakerOrFlask.y = 0f;

        float distFromTip2 = Vector3.Distance(pipetteTip, beakerOrFlask);

        if (closestBeakerOrFlask && distFromTip2 <= ALUMINUM_DROPOFF_RANGE && scoopula.transform.Find("Aluminum").gameObject.activeInHierarchy){ // We have a beaker or flask within range
            Debug.Log("Drop in this beaker");
            scoopula.transform.Find("Aluminum").gameObject.SetActive(false);
            closestBeakerOrFlask.GetComponent<liquidScript>().addSolution(new List<float>{0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f}, 0.7407f);  // Add 0.37 mL of Aluminum
            closestBeakerOrFlask.GetComponent<Rigidbody>().AddForce(Vector3.up * 0.0001f, ForceMode.Impulse);
        }
    }

    IEnumerator getAluminumUsingScoopula(GameObject container)
    {
        scoopulaAnimationPlaying = true;
        Transform cap = container.transform.Find("Cap");
        GetComponent<playerMovement>().canMove = false;
        GetComponent<playerMovement>().canTurn = false;
        GetComponent<pickUpObjects>().canRotateItem = false;
        // Debug.Log(GetComponent<pickUpObjects>().targetRotation.y);

        if (cap == null)
            yield break; // Stop the coroutine if cap is not found

        float speedMult = 1/2f;
        Vector3 startPos = cap.position;
        Vector3 targetPos = startPos + cap.parent.up * 0.08f; // Move up by 0.1 units
        Vector3 leftPos = targetPos - cap.parent.right * 0.14f; // Move left by 0.13 units
        float duration = 0.8f; // Time to move up/down
        float rotationSpeed = 240f; // Degrees per second

        yield return StartCoroutine(MoveAndRotateOverTime(cap, startPos, targetPos, duration * speedMult, -rotationSpeed, Vector3.zero));
        yield return new WaitForSeconds(duration * speedMult);
        Vector3 tiltRotation = new Vector3(0f, 0f, 70f); // Tilt 30 degrees on Z-axis
        yield return StartCoroutine(MoveAndRotateOverTime(cap, targetPos, leftPos, duration * speedMult, 0, tiltRotation));

        // Wait for 2 seconds THIS IS THE TIME WHERE THE THING DIPS IN AND OUT
        
        
        StartCoroutine(LerpValue(value => GetComponent<pickUpObjects>().targetX = value, 0f, 30f, 1.2f * speedMult));
        yield return new WaitForSeconds(1.2f * speedMult);
        GetComponent<pickUpObjects>().other.transform.Find("Aluminum").gameObject.SetActive(true); // enable aluminum pellets

        
        yield return new WaitForSeconds(0.7f * speedMult);
        StartCoroutine(LerpValue(value => GetComponent<pickUpObjects>().targetX = value, 30f, 0f, 0.5f * speedMult));
        yield return new WaitForSeconds(0.8f * speedMult);
        

        Vector3 tiltRotationBack = new Vector3(0f, 0f, -70f); // Tilt 30 degrees on Z-axis
        yield return StartCoroutine(MoveAndRotateOverTime(cap, leftPos, targetPos, duration * speedMult, 0, tiltRotationBack));
        yield return new WaitForSeconds(duration * speedMult);
        yield return StartCoroutine(MoveAndRotateOverTime(cap, targetPos, startPos, duration * speedMult, rotationSpeed, Vector3.zero));
        yield return new WaitForSeconds(duration * speedMult);

        scoopulaAnimationPlaying = false;
        cap.localPosition = new Vector3(0f, 0.3299f, 0f);
        cap.localEulerAngles = new Vector3(0f, 0f, 0f);
        GetComponent<playerMovement>().canMove = true;
        GetComponent<playerMovement>().canTurn = true;
        GetComponent<pickUpObjects>().canRotateItem = true;
    }

    IEnumerator MoveAndRotateOverTime(Transform obj, Vector3 start, Vector3 end, float duration, float rotationSpeed, Vector3 targetRotation)
    {
        float elapsedTime = 0f;
        Quaternion startRotation = obj.rotation;
        Quaternion endRotation = obj.rotation * Quaternion.Euler(targetRotation);

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            obj.position = Vector3.Lerp(start, end, t);
            
            // Rotate smoothly if a rotation is applied
            if (rotationSpeed != 0)
                obj.Rotate(obj.transform.parent.up, rotationSpeed * Time.deltaTime);
            else
                obj.rotation = Quaternion.Lerp(startRotation, endRotation, t); // Tilt smoothly

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        obj.position = end; // Ensure final position is set
        obj.rotation = endRotation; // Ensure final rotation is set
    }

    IEnumerator LerpValue(System.Action<float> setValue, float start, float end, float duration)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            setValue(Mathf.Lerp(start, end, t));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        setValue(end); // Ensure it reaches the exact target value
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
