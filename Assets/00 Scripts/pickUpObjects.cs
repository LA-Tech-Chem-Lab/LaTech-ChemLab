using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using Obi;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.GraphicsBuffer;
using System.Linq;

public class pickUpObjects : MonoBehaviour
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
    public AudioClip puttingGogglesOnClip;


    
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
    public GameObject stuffInEyesFilter;


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
        setTargetPosition();

        checkForInput();
        if (checkForCollisions) PreventWeirdIntersections();
        maintainObject();
        handleObjectShadow();

        if (holdingItem) setHelpTextConstantly();

        if(stuffInEyesFilter.activeSelf){
            multiHandlerScript.setHelpText("Warning! It looks like you were standing near a violent reaction without your protective goggles and you have some in your eyes. Immediately use the eye wash station to evacuate the contaminent.");
        }
    }

    public void PickUpItem(GameObject otherObject)
    {
        if (!otherObject) return; 

        holdingItem = true;
        other = otherObject;
        
        // ChangeOwnershipServerRpc(other.GetComponent<NetworkObject>().NetworkObjectId, NetworkManager.Singleton.LocalClientId);

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


        ////////////////////////////////    Item Specific Stuff    //////////////////////////////////////////////////
        
        if (other.name == "Pipette"){
            initialHoldingDistance = 1.3f;
            canRotateItem = false;
            GetComponent<doCertainThingWith>().heldPipette = other;
        }

        if (other.name == "Thrown Match(Clone)"){
            initialHoldingDistance = 1.3f;
            canRotateItem = false;
        }

        if (other.name == "Bunsen Burner")
            initialHoldingDistance = 1.8f;
        
        if (other.name == "Glass Funnel"){
            if (GetComponent<doCertainThingWith>().funnelIsAttatched == true)
                GetComponent<doCertainThingWith>().DetachFunnel(other);
        }

        if (other.name == "Paper Cone"){
            if (GetComponent<doCertainThingWith>().filterIsAttatched == true || GetComponent<doCertainThingWith>().buchnerfilterIsAttached == true)
                GetComponent<doCertainThingWith>().DetachFilter(other);
        }

        if (other.name == "Buchner Funnel"){
            if (GetComponent<doCertainThingWith>().buchnerfunnelIsAttached == true){
                GetComponent<doCertainThingWith>().DetachBuchnerFunnel(other);
            }
        }

        if (other.name == "Stir Rod"){
            initialHoldingDistance = 1.3f;
            if (GetComponent<doCertainThingWith>().isRodInBeaker == true)
                GetComponent<doCertainThingWith>().removeStirRod(other);
        }

        if (other.name == "Small Stir Rod"){
            initialHoldingDistance = 1.3f;
            if (GetComponent<doCertainThingWith>().isSmallRodInBeaker == true)
                GetComponent<doCertainThingWith>().removeStirRod(other);
        }
        
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

        if (other.name == "Pipette") {
            other.transform.Find("Tip").GetComponent<ObiEmitter>().speed = 0f;   
            GetComponent<doCertainThingWith>().turnOffBeakers();
            other.tag = "Untagged";
        }

        if (other.name.StartsWith("Beaker") || other.name.StartsWith("Erlenmeyer Flask") || other.name == "Scoopula" ||  other.name == "Weigh Boat" || other.name == "Buchner Flask" || other.name == "Graduated Cylinder" || other.name == "Paper Cone"){
            GetComponent<doCertainThingWith>().turnOffBeakers();
        }
        other = null;holdingItem = false;
        multiHandlerScript.setHelpText("");
    }

    void setHelpTextBasedOnObject(){
        if (other.name == "Fire extinguisher")      multiHandlerScript.setHelpText("Right click to use."); 
        if (other.name == "Tongs")                  multiHandlerScript.setHelpText("Right click to grab a flask.");
        if (other.name == "Erlenmeyer Flask 250")   multiHandlerScript.setHelpText("250 mL Erlenmeyer flask");
        if (other.name == "Erlenmeyer Flask 500")   multiHandlerScript.setHelpText("500 mL Erlenmeyer flask");
        if (other.name == "Weigh Boat")             multiHandlerScript.setHelpText("This is a weigh boat. You can use it to measure out the aluminum pellets on the scale. When you have the right amount, you can dump its contents into a beaker.");
        if (other.name == "Glass Funnel")           multiHandlerScript.setHelpText("This is a glass funnel used for filtering out solids from solutions. Right click on an Erlenmeyer flask to attatch it.");
        //if (other.name == "Paper Cone")             multiHandlerScript.setHelpText("This is a paper filter used with a funnel to filter solids from a solution.");
        if (other.name == "Buchner Funnel")         multiHandlerScript.setHelpText("This is a Buchner funnel used for filtering out solids from solutions. Right click on an Buchner flask to attatch it.");
        if (other.name == "Aluminum Container")     multiHandlerScript.setHelpText("This is a container of aluminum pellets. Use the scoopula to scoop the pellets into a weigh boat.");
    }

    void setHelpTextConstantly(){
        if (other.name == "Pipette"){
            pipetteScript ps = other.GetComponent<pipetteScript>();
            multiHandlerScript.setHelpText($"{ps.pipetteVolume} / {ps.pipetteMaxVolume} mL");
            if (GetComponent<doCertainThingWith>().tryingToPipetteSolid){
                multiHandlerScript.setHelpText("It looks like you are trying to pipette a solid. Maybe try pouring this substance by picking up the container and right clicking another container.");
            }
        }

        if (other.name == "Scoopula"){
            string contents = "";
            if (other.transform.Find("Aluminum").gameObject.activeInHierarchy){
                contents = "Aluminum";
            }
            multiHandlerScript.setHelpText("Scoopula: \nContains: " + contents);
            if (GetComponent<doCertainThingWith>().tryingToMixCompoundsInNonLiquidHolder){
                multiHandlerScript.setHelpText("It looks like you are trying to mix two different types of compounds in a non-mixing container. This is not allowed. Try using a clean dish");
            }
        }

        if (other.name == "Bunsen Burner"){
            if (Input.GetMouseButton(1))
                multiHandlerScript.setHelpText($"Scroll to adjust airflow\n{other.GetComponent<bunsenBurnerScript>().airflow.ToString("F2")}/1");
            else
                multiHandlerScript.setHelpText("Right Click to adjust airflow");
        }

        if (other.name == "Graduated Cylinder"){
            if (Input.GetMouseButton(1)){
                liquidScript LS = other.GetComponent<liquidScript>();
                multiHandlerScript.setHelpText("Volume: " + LS.currentVolume_mL + " mLs");
            }
            else{
                multiHandlerScript.setHelpText("This is a Graduated cylinder. Hold right click to observe the volume of your solution.");
            }
        }

        if (other.name.StartsWith("Beaker") || other.name.StartsWith("Erlenmeyer Flask") || other.name.StartsWith("Paper Cone") || other.name.StartsWith("Buchner Flask")){
            /* if (other.transform.Find("Melting Point Tool") != null)
            {
                if (Input.GetMouseButton(1))
                {
                    if (other.transform.Find("Melting Point Tool") != null)
                    {
                        GameObject meltingPointTool = other.transform.Find("Melting Point Tool").gameObject;
                        GameObject capillaryTube = meltingPointTool.transform.Find("Capilary tube (1)")?.gameObject;

                        if (capillaryTube != null)
                        {
                            liquidScript capillaryLiquid = capillaryTube.GetComponent<liquidScript>();
                            liquidScript beakerLiquid = other.GetComponent<liquidScript>();

                            if (capillaryLiquid != null && beakerLiquid != null)
                            {
                                float waterTemp = beakerLiquid.liquidTemperature - 273.15f; // Convert from Kelvin to Celsius
                                float meltingPoint = capillaryLiquid.GetMeltingPoint(); // Assume a method to fetch melting point

                                // Approximate heating process (simple interpolation)
                                float heatTransferRate = 0.05f; // Adjust this for realism
                                float capillaryTemp = Mathf.Lerp(25f, waterTemp, Time.timeSinceLevelLoad * heatTransferRate);

                                string helpText;
                                if (capillaryTemp < meltingPoint)
                                {
                                    helpText = $"Heating up... Current Temperature: {capillaryTemp:F1}°C";
 
                                }
                                else
                                {
                                    helpText = $"Melting point reached! The substance is melting at {meltingPoint:F1}°C.";
                                }
                                multiHandlerScript.setHelpText(helpText);
                            }
                        }
                    }
                }
                else
                {
                    multiHandlerScript.setHelpText($"This is a {other.GetComponent<liquidScript>().totalVolume_mL} mL beaker. Hold right click to observe its contents. You can also hold P to pour into another container.");
                }
                */
            if (Input.GetMouseButton(1))
            {
                string helpText = "Contents: \n";
                liquidScript LS = other.GetComponent<liquidScript>();
                List<float> solutionMols = Enumerable.Repeat(0f, 11).ToList();

                // Convert percentages to moles for reactants
                for (int i = 0; i < solutionMols.Count; i++)
                {
                    float reactantMol = LS.solutionMakeup[i] * LS.densityOfLiquid / LS.molarMasses[i] * 1000;
                    solutionMols[i] = reactantMol;
                }

                for (int i = 0; i < LS.solutionMakeup.Count; i++)
                {
                    if (LS.solutionMakeup[i] > 0.001)
                    {
                        helpText += LS.compoundNames[i];
                        helpText += ": ";
                        helpText += (solutionMols[i]).ToString("F2");
                        helpText += " M\n";
                    }
                }
                helpText += (LS.liquidTemperature - 273.15f) + "°C.\n";
                multiHandlerScript.setHelpText(helpText);
            }
            else
            {
                if (other.name.StartsWith("Beaker")){
                    multiHandlerScript.setHelpText($"This is a {other.GetComponent<liquidScript>().totalVolume_mL} mL beaker. Hold right click to observe its contents. You can also hold P to pour into another container.");
                }
                else if (other.name.StartsWith("Erlenmeyer Flask")){
                    multiHandlerScript.setHelpText($"This is a {other.GetComponent<liquidScript>().totalVolume_mL} mL Erlenmeyer Flask. Hold right click to observe its contents. You can also hold P to pour into another container.");
                }
                else if (other.name.StartsWith("Buchner Flask")){
                    multiHandlerScript.setHelpText($"This is a {other.GetComponent<liquidScript>().totalVolume_mL} mL Buchner Flask. Hold right click to observe its contents. You can also hold P to pour into another container.");
                }
                else if (other.name.StartsWith("Paper Cone")){
                    multiHandlerScript.setHelpText($"This is a {other.GetComponent<liquidScript>().totalVolume_mL} mL Paper Cone. Hold right click to observe its contents. You can also hold P to pour into another container.");
                }
            }
            if (GetComponent<doCertainThingWith>().tryingToPourLiquidOnPaperTowel){
                multiHandlerScript.setHelpText("This solution is too wet to pour onto a paper towel.");
            }
        }
        if (other.name.StartsWith("Paper Towel")){
            if (Input.GetMouseButton(1)){
                string helpText = "Contents: \n";
                weighboatscript WBS = other.GetComponent<weighboatscript>();
                List<float> solutionMols = Enumerable.Repeat(0f, 11).ToList();
                // Convert percentages to moles for reactants
                for (int i = 0; i < solutionMols.Count; i++)
                {
                    float reactantMol = WBS.solutionMakeup[i] * WBS.density / WBS.molarMasses[i] * 1000;
                    solutionMols[i] = reactantMol;
                }
                for (int i = 0; i < WBS.solutionMakeup.Count; i++)
                {
                    if (WBS.solutionMakeup[i] > 0.001)
                    {
                        helpText += WBS.compoundNames[i];
                        helpText += ": ";
                        helpText += (solutionMols[i]).ToString("F2");
                        helpText += " M\n";
                    }
                }
                multiHandlerScript.setHelpText(helpText);
            }
            else{
                multiHandlerScript.setHelpText("This is a paper towel. You can pour or scoop dry-ish substanses onto them in order to dry them further.");
            }
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

        if (!Cursor.visible && canRotateItem) targetRotation.y += Input.GetAxis("Mouse X") * xSens * Time.deltaTime;
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
            if (hitObject.GetComponent<doorScript>() || hitObject.GetComponent<doorScriptXAxis>() || hitObject.GetComponent<cabinetScript>())
            {
                return; // Simply exit the function, preventing pickup
            }

            Rigidbody rb = hitObject.GetComponent<Rigidbody>();

            if (rb && rb.GetComponent<Rigidbody>().isKinematic) // We are trying to pick up a kinematic object - not normal
            {
                // if (hit.collider.gameObject.tag == "IronRing"){
                //     DetachIronRingServerRpc(netObj.NetworkObjectId);
                //     return;
                // }
                // Debug.Log(hit.collider.gameObject.name);
            }

            // Can also be the funnel even if it is kinematic because we want to be able to pick it up when it is attatched to the flask
            if (rb && hitObject.tag != "NoPickup") // ITEM PICKUP
            {
                PickUpItem(hitObject);
            }

            if (hitObject.tag == "NoPickup")
                alternativeToPickingUp(hitObject);
        }
    }

    void alternativeToPickingUp(GameObject hitObject){
        if (hitObject.name == "Paper Towel"){
            GetComponent<doCertainThingWith>().givePlayerPaperTowelSheet(hitObject.transform);
        }

        if (hitObject.name == "Hanging Goggles" && !GetComponent<interactWithObjects>().gogglesOn){
            hitObject.SetActive(false);
            GetComponent<interactWithObjects>().gogglesOn = true;

            // Play audio
            GameObject tempAudio = new GameObject("TempAudio");
            AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
            audioSource.clip = puttingGogglesOnClip;
            audioSource.volume = 0.6f;
            audioSource.Play();
            Destroy(tempAudio, puttingGogglesOnClip.length); // Cleanup
        }

        if (hitObject.name == "Goggles Hanger" && !hitObject.transform.Find("Hanging Goggles").gameObject.activeInHierarchy){
            hitObject.transform.Find("Hanging Goggles").gameObject.SetActive(true);
            GetComponent<interactWithObjects>().gogglesOn = false;
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
                if (other.name != "Pipette" && other.name != "Thrown Match(Clone)") initialHoldingDistance = Mathf.MoveTowards(initialHoldingDistance, initialHeldDistForObject, blendingSensitivity * Time.deltaTime);
                // if (distFromGroundLayer < holdingDistance) 
                //     if (other.name != "Matchbox" && other.name != "Tongs" && other.name != "Glass Plate" && !other.name.StartsWith("Beaker")) { 
                //         Debug.Log("If you are seeing this and the item is not behind a wall, go to pickUpObjects > PreventWeirdIntersections() and add this name to the if statement"); holdingDistance = distFromGroundLayer - checkRadius; initialHoldingDistance = holdingDistance; }  // Look the tongs were a problem for some reason
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

            if (other.GetComponent<liquidScript>() == null || !other.GetComponent<liquidScript>().isPouring)
            {
                other.transform.position = Vector3.Slerp(
                    other.transform.position,
                    targetPosition, 
                    Time.deltaTime * blendingSensitivity
                );
            }

            heldObjPosition = other.transform.position;

            // Allow objects to stay tipped while pouring
            if (other.GetComponent<liquidScript>() == null || !other.GetComponent<liquidScript>().isPouring)
            {
                other.transform.localRotation = Quaternion.Slerp(
                    other.transform.localRotation,
                    targetQuaternion,
                    Time.deltaTime * blendingSensitivity
                );
            }

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
        
            // Draw the holding position sphere
            Gizmos.color = holdingItem ? Color.green : Color.red;
            if (other)
                Gizmos.DrawSphere(targetPosition + other.transform.TransformDirection(targetPositionShift), checkRadius);
            else
                Gizmos.DrawSphere(targetPosition + targetPositionShift, checkRadius);
    }

    public void DetachIronRingFromStand(GameObject ironRing)
    {

        DetachIronRing(ironRing);
    }


    private void DetachIronRing(GameObject ironRing)
    {
        if (ironRing == null)
        {
            Debug.LogError("DetachIronRing: ironRing is null!");
            return;
        }

        Debug.Log($"DetachIronRing: Attempting to detach {ironRing.name}");

        Rigidbody rb = ironRing.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
            Debug.Log($"DetachIronRing: Rigidbody found, set isKinematic to {rb.isKinematic}");
        }
        else
        {
            Debug.LogWarning("DetachIronRing: Rigidbody not found on object!");
        }

        Debug.Log($"DetachIronRing: Parent before change: {ironRing.transform.parent}");
        ironRing.transform.SetParent(null);
        Debug.Log($"DetachIronRing: Parent after change: {ironRing.transform.parent}");

        Transform ring = ironRing.transform.Find("Ring");
        if (ring)
        {
            ring.localPosition = new Vector3(0.0256326012f, 0f, 0.0123416251f);
            Debug.Log("DetachIronRing: Ring position reset");
        }
        else
        {
            Debug.LogWarning("DetachIronRing: Ring not found!");
        }

        Transform screw = ironRing.transform.Find("Screw");
        if (screw)
        {
            screw.localPosition = new Vector3(-0.161500007f, -0.000211842445f, 0.0122618228f);
            Debug.Log("DetachIronRing: Screw position reset");
        }
        else
        {
            Debug.LogWarning("DetachIronRing: Screw not found!");
        }

        BoxCollider collider = ironRing.GetComponent<BoxCollider>();
        if (collider)
        {
            collider.center = new Vector3(0f, 0.001574993f, 0.015f);
            Debug.Log("DetachIronRing: BoxCollider center updated");
        }
        else
        {
            Debug.LogWarning("DetachIronRing: BoxCollider not found!");
        }

        Transform ironMesh = ironRing.transform.Find("Iron Mesh");
        if (ironMesh)
        {
            Rigidbody ironMeshRb = ironMesh.GetComponent<Rigidbody>();
            if (ironMeshRb)
            {
                ironMeshRb.isKinematic = false;
                Debug.Log($"DetachIronRing: Iron Mesh Rigidbody set isKinematic to {ironMeshRb.isKinematic}");
            }
            else
            {
                Debug.LogWarning("DetachIronRing: Iron Mesh Rigidbody not found!");
            }

            Debug.Log($"DetachIronRing: Iron Mesh Parent before change: {ironMesh.parent}");
            ironMesh.SetParent(null);
            Debug.Log($"DetachIronRing: Iron Mesh Parent after change: {ironMesh.parent}");
        }
        else
        {
            Debug.LogWarning("DetachIronRing: Iron Mesh not found!");
        }
    }
}