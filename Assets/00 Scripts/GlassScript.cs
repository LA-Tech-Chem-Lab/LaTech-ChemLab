using Unity.Netcode; // Import Netcode for GameObjects
using UnityEngine;

public class GlassScript : MonoBehaviour
{
    GameObject unbroken;
    GameObject broken;
    public float breakThreshold = 300f; // Example threshold

    private bool isBroken = false;

    void Start()
    {
        unbroken = transform.Find("Unbroken").gameObject;
        broken = transform.Find("Broken").gameObject;

    }

    void OnCollisionEnter(Collision collision)
    {
        if (unbroken.activeInHierarchy)
        {
            Rigidbody objectRB = collision.gameObject.GetComponent<Rigidbody>();
            if (objectRB != null)
            {
                // Store velocity
                Vector3 originalVelocity = objectRB.linearVelocity;

                // Calculate collision force
                Vector3 relativeVelocity = collision.relativeVelocity;
                float forceMagnitude = relativeVelocity.magnitude * objectRB.mass;

                if (forceMagnitude > breakThreshold)
                {
                    GetComponent<BoxCollider>().enabled = false;
                    unbroken.SetActive(false);
                    broken.SetActive(true);

                    // Activate physics on the broken glass pieces
                    foreach (Transform child in broken.transform)
                    {
                        Rigidbody rb = child.GetComponent<Rigidbody>();
                        if (rb != null)
                            rb.isKinematic = false;
                    }

                    // Prevent bounce by reapplying original velocity
                    objectRB.linearVelocity = originalVelocity;

                    // Ignore future collisions with the glass
                    Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
                }
            }
        }
    }



}
