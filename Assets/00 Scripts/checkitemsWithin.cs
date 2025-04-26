using UnityEngine;
using System.Collections.Generic;

public class CheckItemsWithin : MonoBehaviour
{
    private BoxCollider[] colliders;
    public Transform spawnPoint;

    void Start()
    {
        colliders = GetComponents<BoxCollider>();
        spawnPoint = transform.Find("SpawnPoint");
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
                    if (obj.layer != LayerMask.NameToLayer("IgnoreGround") && obj.layer != LayerMask.NameToLayer("Ignore Raycast"))
                        objectsToMove.Add(obj);
                }
            }
        }

        foreach (GameObject obj in objectsToMove)
        {   
            if (obj.layer == LayerMask.NameToLayer("HeldObject"))
                return;
                
            obj.transform.Translate(Vector3.up * 0.02f);
            if (obj.GetComponent<Rigidbody>()) obj.GetComponent<Rigidbody>().linearVelocity = Vector3.up;
        }
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
