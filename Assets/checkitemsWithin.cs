using UnityEngine;
using System.Collections.Generic;

public class CheckItemsWithin : MonoBehaviour
{
    private BoxCollider[] colliders;

    void Start()
    {
        colliders = GetComponents<BoxCollider>();
    }

    void Update()
    {
        MoveObjectsOutOfBounds();
    }

    void MoveObjectsOutOfBounds()
    {
        HashSet<GameObject> objectsToMove = new HashSet<GameObject>();

        foreach (BoxCollider box in colliders)
        {
            Vector3 center = box.bounds.center;
            Vector3 halfExtents = box.bounds.extents;

            Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity);

            foreach (Collider hit in hits)
            {
                GameObject obj = hit.gameObject;

                if (obj != gameObject && !IsDescendantOf(obj.transform, transform)) // Ignore self & descendants
                {
                    objectsToMove.Add(obj);
                }
            }
        }

        foreach (GameObject obj in objectsToMove)
        {
            MoveObjectOut(obj);
        }
    }

    void MoveObjectOut(GameObject obj)
    {
        Collider objCollider = obj.GetComponent<Collider>();
        if (objCollider == null) return;

        Vector3 closestEscapePoint = FindClosestEscape(obj.transform.position);
        Vector3 moveDirection = (closestEscapePoint - obj.transform.position).normalized;
        float moveDistance = Vector3.Distance(obj.transform.position, closestEscapePoint);

        // Move object to the escape point
        obj.transform.position += moveDirection * moveDistance * 1.5f;
        Debug.Log(obj.name + " moved to escape point: " + closestEscapePoint);
    }

    Vector3 FindClosestEscape(Vector3 objectPosition)
    {
        Vector3 closestEscape = Vector3.zero;
        float minDistance = float.MaxValue;

        foreach (BoxCollider box in colliders)
        {
            Vector3[] escapePoints = new Vector3[]
            {
                box.bounds.center + new Vector3(box.bounds.extents.x, 0, 0),  // Right
                box.bounds.center - new Vector3(box.bounds.extents.x, 0, 0),  // Left
                box.bounds.center + new Vector3(0, 0, box.bounds.extents.z),  // Forward
                box.bounds.center - new Vector3(0, 0, box.bounds.extents.z)   // Backward
            };

            foreach (Vector3 escape in escapePoints)
            {
                float distance = Vector3.Distance(objectPosition, escape);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEscape = escape;
                }
            }
        }

        return closestEscape;
    }

    bool IsDescendantOf(Transform child, Transform potentialParent)
    {
        while (child != null)
        {
            if (child == potentialParent)
                return true;
            child = child.parent;
        }
        return false;
    }
}
