using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.GraphicsBuffer;

public class pickUpObjects : MonoBehaviour
{
    public Transform playerCamera;
    public float range = 7f;
    public float holdingDistance = 3f; 
    float initialHoldingDistance;
    public float blendingSensitivity = 3f;
    public GameObject other;  int otherObjectLayer;

    bool holdingItem;
    Vector3 prev;
    Vector3 launchTraj;
    Vector3 targetRotation;
    Vector3 meshOffset;
    Vector3 targetPosition;
    Quaternion targetQuaternion;
    float xSens;




    public float checkRadius = 0.5f;
    public bool targetPositionBlocked;


    void Start(){
        xSens = GetComponent<playerMovement>().xSens;
        initialHoldingDistance = holdingDistance;
        playerCamera = transform.GetChild(0);
    }

    void Update()
    {
        targetPosition = playerCamera.position + playerCamera.forward * holdingDistance + meshOffset;

        checkForInput();
        preventWeirdIntersections();
        maintainObject();

    }


    void checkForInput()
    {
        if (Input.GetMouseButtonDown(0)){
            if (!holdingItem)
                checkForRigidbody();
            else { // Dropping Item
                holdingItem = false;
                other.layer = otherObjectLayer;
                other.GetComponent<Rigidbody>().linearVelocity = launchTraj; // Launch it
                other = null;
            }
        }
        
        targetRotation.y += Input.GetAxis("Mouse X") * xSens * Time.deltaTime;
        targetRotation.y -= Mathf.Min(1f, Input.mouseScrollDelta.y) * blendingSensitivity;
        targetRotation.y = (targetRotation.y % 360f + 360f) % 360f;
        targetQuaternion = Quaternion.Euler(targetRotation);
        
    }

    void checkForRigidbody(){
        
        Ray forwardRay = new Ray(playerCamera.transform.position, playerCamera.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(forwardRay, out hit, range)) {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();

            if (rb && !rb.isKinematic) // YES RIGIDBODY WE JUST PICKED UP ITEM
            {
                holdingItem = true;
                other = hit.collider.gameObject;
                otherObjectLayer = other.layer;
                other.layer = LayerMask.NameToLayer("Ignore Raycast");

                // Try mesh stuff
                if (other.GetComponent<MeshFilter>()){
                    // Mesh Offset
                    Vector3 boundsCenterLocal = other.GetComponent<MeshFilter>().sharedMesh.bounds.center;
                    Vector3 boundsCenterWorld = other.transform.TransformPoint(boundsCenterLocal);
                    Vector3 pivotWorld = other.transform.position;
                    meshOffset = pivotWorld - boundsCenterWorld;
                }
                else  meshOffset = Vector3.zero;


                targetRotation = new Vector3(0f, other.transform.localEulerAngles.y, 0f);
                targetQuaternion = Quaternion.Euler(targetRotation);
            }
        }
    }

    void preventWeirdIntersections(){
        if (holdingItem){
            targetPositionBlocked = Physics.CheckSphere(targetPosition, checkRadius,   1 << LayerMask.NameToLayer("Ground") );

            if (targetPositionBlocked){
                Ray forwardRay = new Ray(playerCamera.position, playerCamera.forward);
                RaycastHit hit;

                if (Physics.Raycast(forwardRay, out hit, holdingDistance)){
                    float distFromCamera = Vector3.Distance(playerCamera.position, hit.point);
                    Debug.Log(distFromCamera);
                    holdingDistance -= (holdingDistance - distFromCamera);
                }
            } else {
                holdingDistance = Mathf.MoveTowards(holdingDistance, initialHoldingDistance, blendingSensitivity * Time.deltaTime);
                
                targetPosition = playerCamera.position + playerCamera.forward * holdingDistance + meshOffset;
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
        }
    }


}
