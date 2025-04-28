using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cabinetScript : MonoBehaviour
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

    
    public AudioClip openingSound;
    public AudioClip closingSound;
    

    private Vector3 targetPosition;

    void Start()
    {
        initialPosition = transform.localPosition;
        lastPos = transform.position;
        targetPosition = initialPosition;

        UpdateCabinet(cabinetIsClosed);
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

    public void InteractWithThisCabinet()
    {
        cabinetIsClosed = !cabinetIsClosed;
        playDrawerSound();
        UpdateCabinet(cabinetIsClosed);
    }

    void playDrawerSound(){
        if (cabinetIsClosed && closingSound)
            AudioSource.PlayClipAtPoint(closingSound, transform.position);
        else if (!cabinetIsClosed && openingSound)
            AudioSource.PlayClipAtPoint(openingSound, transform.position);
    }

    private void UpdateCabinet(bool isClosed)
    {
        targetPosition = isClosed ? initialPosition : initialPosition + Vector3.forward * cabinetMovement;
    }

    private void DetectObjectsInBox()
    {
        objectsInArea.Clear();

        Vector3 boxCenter = transform.position + transform.TransformVector(boxOffset);
        Collider[] colliders = Physics.OverlapBox(boxCenter, boxSize / 2, transform.rotation);

        HashSet<GameObject> uniqueObjects = new HashSet<GameObject>();

        foreach (Collider collider in colliders)
        {
            GameObject obj = collider.gameObject;

            if (obj.tag != "Terrain" && obj != gameObject &&
                !collider.transform.IsChildOf(transform.parent.parent) &&
                !collider.transform.IsChildOf(roomMesh.transform))
            {
                uniqueObjects.Add(obj); // Only add each GameObject once
            }
        }

        foreach (GameObject obj in uniqueObjects)
        {
            obj.transform.Translate(cabinetVel, Space.World);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f); // Semi-transparent red
        if (roomMesh != null)
        {
            Vector3 boxCenter = transform.position + transform.TransformVector(boxOffset);
            Quaternion boxRotation = transform.rotation;

            Gizmos.matrix = Matrix4x4.TRS(boxCenter, boxRotation, Vector3.one);
            Gizmos.DrawCube(Vector3.zero, boxSize);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
        }
    }
}