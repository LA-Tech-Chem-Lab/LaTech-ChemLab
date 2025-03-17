using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class doorScript : NetworkBehaviour
{
    public Transform hinge;
    public bool doorIsClosed = true;

    public float closedAngle = 0f;
    public float openAngle = 90f;
    public float blendingSensitivity = 3f;
    bool coroutineRunning = false;

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

        if (transform.Find("Inside Handle Pivot"))
            handles.Add(transform.Find("Inside Handle Pivot").gameObject);
        if (transform.Find("Outside Handle Pivot"))
            handles.Add(transform.Find("Outside Handle Pivot").gameObject);
        
    }

    void Update()
    {
        hinge.localRotation = Quaternion.Slerp(
            hinge.localRotation,
            targetQuaternion,
            Time.deltaTime * blendingSensitivity
        );

    }

    public void InteractWithThisDoor()
    {
            rotateHandles();
            doorState.Value = !doorState.Value; // Toggle door state on server
        
    }

    void rotateHandles(){
        foreach (GameObject g in handles)
                if (g.name == "Inside Handle Pivot")
                    StartCoroutine(RotateHandleCoroutine(g, 0.2f, 90f, 150f));
                else
                    StartCoroutine(RotateHandleCoroutine(g, 0.2f, -90f, -30f));
    }

    private void OnDoorStateChanged(bool previousState, bool newState)
    {
        UpdateDoorRotation(newState);
    }

    private void UpdateDoorRotation(bool isClosed)
    {
        if (isClosed)
        {
            targetRotation = new Vector3(0f, closedAngle, 0f);
        }
        else
        {
            targetRotation = new Vector3(0f, openAngle, 0f);
        }

        targetQuaternion = Quaternion.Euler(targetRotation);
        doorIsClosed = isClosed;
    }

    // private void OnDestroy()
    // {
    //     doorState.OnValueChanged -= OnDoorStateChanged;
    // }



    private IEnumerator RotateHandleCoroutine(GameObject handle, float duration, float rest, float turned)
    {   
        coroutineRunning = true;
        Quaternion startRotation = Quaternion.Euler(rest, 0, 0);
        Quaternion targetRotation = Quaternion.Euler(turned, 0, 0);

        float elapsed = 0;

        // Rotate to the target
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            handle.transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
            yield return null;
        }

        handle.transform.localRotation = targetRotation;

        // Pause briefly
        yield return new WaitForSeconds(0.2f);

        // Rotate back to the start
        elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            handle.transform.localRotation = Quaternion.Slerp(targetRotation, startRotation, elapsed / duration);
            yield return null;
        }

        handle.transform.localRotation = startRotation;
        coroutineRunning = false;
    }

}
