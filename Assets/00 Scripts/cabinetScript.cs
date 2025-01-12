using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class cabinetScript : NetworkBehaviour
{
    private Vector3 initialPosition;
    private Vector3 lastPos;
    public bool cabinetIsClosed = true;

    public float cabinetMovement = 2f;
    public float blendingSensitivity = 3f;
    
    public GameObject roomMesh;
    public Vector3 boxSize;
    public Vector3 boxOffset;

    public Vector3 cabinetVel;

    public List<GameObject> objectsInArea = new List<GameObject>();

    

    // NetworkVariable to sync door state
    private NetworkVariable<bool> cabinetState = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Vector3 targetPosition;

    void Start()
    {
        initialPosition = transform.localPosition;
        lastPos = transform.position;

        targetPosition = initialPosition;
        // Subscribe to state changes
        cabinetState.OnValueChanged += OnCabinetStateChanged;

        // Set the initial state
        UpdateCabinet(cabinetState.Value);
    }

    void FixedUpdate()
    {
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * blendingSensitivity
        );

        
        cabinetVel = transform.position - lastPos;
        lastPos = transform.position;
        
        DetectObjectsInBox();
    }

    // Called when a player interacts with the cabinet
    public void InteractWithThisCabinet()
    {
        if (IsServer)
        {
            cabinetState.Value = !cabinetState.Value; // Toggle cabinet state on server
        }
    }
    
    private void OnCabinetStateChanged(bool previousState, bool newState)
    {
        UpdateCabinet(newState);
    }

    private void UpdateCabinet(bool isClosed)
    {
        if (isClosed)
        {
            targetPosition = initialPosition;
        }
        else
        {
            targetPosition = initialPosition + Vector3.forward * cabinetMovement;
        }

        cabinetIsClosed = isClosed;
    }




    private void OnDrawGizmos()
    {
        // Set the color of the gizmo
        Gizmos.color = Color.green;

        // Calculate the center of the box with the offset
        Vector3 boxCenter = transform.position + transform.TransformVector(boxOffset);

        // Draw the wireframe box
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }

    private void DetectObjectsInBox()
    {
        // Clear the previous list
        objectsInArea.Clear();

        // Calculate the center of the box with the offset
        Vector3 boxCenter = transform.position + transform.TransformVector(boxOffset);

        // Perform the OverlapBox detection
        Collider[] colliders = Physics.OverlapBox(boxCenter, boxSize / 2, transform.rotation);

        // Add detected objects to the list
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != gameObject && !collider.transform.IsChildOf(transform.parent.parent) && !collider.transform.IsChildOf(roomMesh.transform))
                objectsInArea.Add(collider.gameObject);
        }

        foreach (GameObject obj in objectsInArea){
            obj.transform.Translate(cabinetVel, Space.World);
        }
    }






    
}
