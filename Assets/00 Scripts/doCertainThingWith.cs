using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class doCertainThingWith : NetworkBehaviour
{   
    const float TONG_GRAB_DISTANCE = 3f;
    const float PIPETTE_GRAB_DISTANCE = 1.2f;


    public GameObject itemHeldByTongs; int itemHeldByTongsLayer;
    pickUpObjects pickUpScript;
    public Vector3 testingOffset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pickUpScript = GetComponent<pickUpObjects>();
    }

    // Update is called once per frame
    void Update()
    {   
        if (!IsOwner)
            return;
            
        checkForInput();

        if (itemHeldByTongs)
            handleTongObject();
    }

    void checkForInput(){
        if (Input.GetMouseButtonDown(1))
            findObjectAndPerformAction();

        if (Input.GetMouseButton(1))
            findObjectAndPerformHeldAction();
    }

    void findObjectAndPerformAction()       // Right click once
    { 
        if (pickUpScript.other != null){
            GameObject obj = pickUpScript.other;

            if (obj.name == "Fire extinguisher")
                ShootFoam();

            if (obj.name == "Tongs")
                GrabFlaskByNeck(obj);

            if (obj.name == "Pipette")
                PipetteStuff(obj);

        }
    }

    void findObjectAndPerformHeldAction()  // Held right click
    {
        if (pickUpScript.other != null)
        {
            GameObject obj = pickUpScript.other;

            if (obj.name == "Beaker")
                BringObjectCloser();
            
            if (obj.name == "Evaporating Dish")
                BringObjectCloser(-1.5f);
            
            if (obj.name == "Erlenmeyer Flask" || obj.name == "Erlenmeyer Flask L")
                BringObjectCloser(-1.5f);

        }
    }










    void BringObjectCloser(float dist = 0f)
    {   
        if (dist <= 0f)
            pickUpScript.distOffset = -pickUpScript.holdingDistance * 0.75f;
        else
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




    public void PipetteStuff(GameObject pipette){

        // First find the closest beaker/flask below you
        float minDist = Mathf.Infinity;
        GameObject closestBeakerOrFlask = null;
        
        foreach (GameObject currentObject in FindObjectsOfType<GameObject>()){
            
            if (currentObject.name == "Beaker" || currentObject.name == "Erlenmeyer Flask" || currentObject.name == "Erlenmeyer Flask L"){

                var pipetteTip = pipette.transform.Find("Tip").transform.position; pipetteTip.y = 0f;
                var beakerOrFlask = currentObject.transform.position;              beakerOrFlask.y = 0f;

                float distFromTip = Vector3.Distance(pipetteTip, beakerOrFlask);
                
                if (distFromTip < minDist){
                    minDist = distFromTip;
                    closestBeakerOrFlask = currentObject;
                }
            }
        }

        if (closestBeakerOrFlask && minDist <= PIPETTE_GRAB_DISTANCE){ // We have a beaker or flask within range

            // Add or subtract liquid from beaker based on volume within pipette
            if (closestBeakerOrFlask.transform.Find("Liquid")){
                
                closestBeakerOrFlask.GetComponent<liquidScript>().currentVolume_mL += 50;
                closestBeakerOrFlask.GetComponent<Rigidbody>().AddForce(Vector3.up * 0.0001f, ForceMode.Impulse);
            }
        }
    }







    void ShootFoam(){
        if (!pickUpScript.other.transform.Find("Foam").GetComponent<ParticleSystem>().isPlaying)
        {
            ParticleSystem foamSpray = pickUpScript.other.transform.Find("Foam").GetComponent<ParticleSystem>();
            foamSpray.Play();
            StartCoroutine(OnOrOffForDelay(pickUpScript.other.transform.Find("Spraying").gameObject, foamSpray.main.duration));
            StartCoroutine(OnOrOffForDelay(pickUpScript.other.transform.Find("Not Spraying").gameObject, foamSpray.main.duration, false));
        }
    }
    
    IEnumerator OnOrOffForDelay(GameObject obj, float delayTime, bool initialState = true)
    {
        obj.SetActive(initialState);
        yield return new WaitForSeconds(delayTime);
        obj.SetActive(!initialState);
    }

    
}
