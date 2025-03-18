using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doorScriptXAxis : MonoBehaviour
{
    public Transform hinge;
    public bool doorIsClosed = true;

    public float closedAngle = 0f;
    public float openAngle = 90f;
    public float blendingSensitivity = 3f;
    bool coroutineRunning = false;

    public float givenZ = 0f;

    private bool _doorState = true;
    public bool DoorState
    {
        get => _doorState;
        set
        {
            if (_doorState != value)
            {
                _doorState = value;
                UpdateDoorRotation(_doorState);
            }
        }
    }

    Vector3 targetRotation;
    Quaternion targetQuaternion;
    List<GameObject> handles = new List<GameObject>();

    void Start()
    {
        hinge = transform.parent;
        UpdateDoorRotation(DoorState);

        string[] handleNames = { "Inside Handle Pivot", "Outside Handle Pivot" };
        foreach (string name in handleNames)
        {
            Transform handleTransform = transform.Find(name);
            if (handleTransform)
                handles.Add(handleTransform.gameObject);
        }
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
        DoorState = !DoorState;
    }

    void rotateHandles()
    {
        foreach (GameObject g in handles)
        {
            float rest = (g.name == "Inside Handle Pivot") ? 90f : -90f;
            float turned = (g.name == "Inside Handle Pivot") ? 150f : -30f;
            StartCoroutine(RotateHandleCoroutine(g, 0.2f, rest, turned));
        }
    }

    private void UpdateDoorRotation(bool isClosed)
    {
        targetRotation = new Vector3(isClosed ? closedAngle : openAngle, 0f, givenZ);
        targetQuaternion = Quaternion.Euler(targetRotation);
        doorIsClosed = isClosed;
    }

    private IEnumerator RotateHandleCoroutine(GameObject handle, float duration, float rest, float turned)
    {
        coroutineRunning = true;
        Quaternion startRotation = Quaternion.Euler(rest, 0, 0);
        Quaternion targetRotation = Quaternion.Euler(turned, 0, 0);

        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            handle.transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
            yield return null;
        }

        handle.transform.localRotation = targetRotation;
        yield return new WaitForSeconds(0.2f);

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
