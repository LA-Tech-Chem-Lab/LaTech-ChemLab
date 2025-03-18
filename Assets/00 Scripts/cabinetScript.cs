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
        UpdateCabinet(cabinetIsClosed);
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

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.tag != "Terrain" && collider.gameObject != gameObject &&
                !collider.transform.IsChildOf(transform.parent.parent) &&
                !collider.transform.IsChildOf(roomMesh.transform))
            {
                objectsInArea.Add(collider.gameObject);
            }
        }

        foreach (GameObject obj in objectsInArea)
        {
            obj.transform.Translate(cabinetVel, Space.World);
        }
    }
}