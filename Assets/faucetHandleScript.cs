using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class faucetHandleScript : NetworkBehaviour
{
    public Transform hinge;
    public bool doorIsClosed = true;

    public float closedAngle = 0f;
    public float openAngle = 90f;
    public float blendingSensitivity = 3f;
    bool coroutineRunning = false;

    public float givenY = 0f;
    public float givenZ = 0f;

    // NetworkVariable to sync door state
    private NetworkVariable<bool> doorState = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    Vector3 targetRotation;
    Quaternion targetQuaternion;

    List<GameObject> handles = new List<GameObject>();

    void Start()
    {

        hinge = transform.parent.gameObject.transform;

        // Subscribe to state changes
        doorState.OnValueChanged += OnDoorStateChanged;

        // Set the initial state
        UpdateDoorRotation(doorState.Value);

        
    }

    void Update()
    {
        hinge.localRotation = Quaternion.Slerp(
            hinge.localRotation,
            targetQuaternion,
            Time.deltaTime * blendingSensitivity
        );

    }

    public void InteractWithThisFaucet()
    {
        if (IsServer)
        {
            doorState.Value = !doorState.Value; // Toggle door state on server
        }
    }

    private void OnDoorStateChanged(bool previousState, bool newState)
    {
        UpdateDoorRotation(newState);
    }

    private void UpdateDoorRotation(bool isClosed)
    {
        if (isClosed)
        {
            targetRotation = new Vector3(0f, 0f, closedAngle);
        }
        else
        {
            targetRotation = new Vector3(0f, 0f, openAngle);
        }

        targetQuaternion = Quaternion.Euler(targetRotation);
        doorIsClosed = isClosed;
    }

    private void OnDestroy()
    {
        doorState.OnValueChanged -= OnDoorStateChanged;
    }



}
