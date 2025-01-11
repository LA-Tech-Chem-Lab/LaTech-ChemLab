using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class cabinetScript : NetworkBehaviour
{
    private Vector3 initialPosition;
    public bool cabinetIsClosed = true;

    public float cabinetMovement = 2f;
    public float blendingSensitivity = 3f;
    public bool fancy;

    // NetworkVariable to sync door state
    private NetworkVariable<bool> cabinetState = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Vector3 targetPosition;

    void Start()
    {
        initialPosition = transform.localPosition;
        targetPosition = initialPosition;
        // Subscribe to state changes
        cabinetState.OnValueChanged += OnCabinetStateChanged;

        // Set the initial state
        UpdateCabinet(cabinetState.Value);
    }

    void Update()
    {
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * blendingSensitivity
        );
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
            if (fancy) targetPosition = initialPosition + transform.up * cabinetMovement;
            else targetPosition = initialPosition + transform.right * cabinetMovement;
        }

        cabinetIsClosed = isClosed;
    }
}
