using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doorScript : MonoBehaviour
{
    public Transform hinge;
    public bool doorIsClosed = true;

    public float closedAngle = 0f;
    public float openAngle = 90f;
    public float blendingSensitivity = 3f;
    public AudioClip openingSound;
    public AudioClip closingSound;
    
    bool coroutineRunning = false;

    private bool doorState = true; // Local state tracking

    Vector3 targetRotation;
    Quaternion targetQuaternion;

    List<GameObject> handles = new List<GameObject>();

    void Start()
    {
        hinge = transform.parent.gameObject.transform;

        // Set the initial state
        UpdateDoorRotation(doorState);

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
        doorState = !doorState; // Toggle door state
        playDoorSound();
        UpdateDoorRotation(doorState);
    }

    void playDoorSound(){
        if (!doorIsClosed && closingSound)
            AudioSource.PlayClipAtPoint(closingSound, transform.position);
        else if (doorIsClosed && openingSound)
            AudioSource.PlayClipAtPoint(openingSound, transform.position);
    }

    void rotateHandles()
    {
        foreach (GameObject g in handles)
        {
            if (g.name == "Inside Handle Pivot")
                StartCoroutine(RotateHandleCoroutine(g, 0.2f, 90f, 150f));
            else
                StartCoroutine(RotateHandleCoroutine(g, 0.2f, -90f, -30f));
        }
    }

    private void UpdateDoorRotation(bool isClosed)
    {
        if (isClosed)
            targetRotation = new Vector3(0f, closedAngle, 0f);
        else
            targetRotation = new Vector3(0f, openAngle, 0f);

        targetQuaternion = Quaternion.Euler(targetRotation);
        doorIsClosed = isClosed;
    }

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
