using Unity.Netcode; // Import Netcode for GameObjects
using UnityEngine;

public class GlassScript : NetworkBehaviour
{
    GameObject unbroken;
    GameObject broken;
    float breakThreshold = 150f; // Example threshold

    private NetworkVariable<bool> isBroken = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Start()
    {
        unbroken = transform.Find("Unbroken").gameObject;
        broken = transform.Find("Broken").gameObject;

        // Update glass state based on the network variable when it changes
        isBroken.OnValueChanged += (previousValue, newValue) =>
        {
            UpdateGlassState(newValue);
        };

        // Initialize the glass state
        UpdateGlassState(isBroken.Value);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsServer && unbroken.activeInHierarchy)
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
                    BreakGlassServerRpc();

                    // Prevent bounce by reapplying original velocity
                    objectRB.linearVelocity = originalVelocity;

                    // Ignore future collisions with the glass
                    Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
                }
            }
        }
    }



    [ServerRpc]
    void BreakGlassServerRpc()
    {
        if (!isBroken.Value) // Ensure it only breaks once
        {
            isBroken.Value = true; // Update the network variable to sync the state
        }
    }

    void UpdateGlassState(bool brokenState)
    {
        if (brokenState)
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
        }
        else
        {
            GetComponent<BoxCollider>().enabled = true;
            unbroken.SetActive(true);
            broken.SetActive(false);

            // Reset physics on the broken glass pieces (optional, for respawning logic)
            foreach (Transform child in broken.transform)
            {
                Rigidbody rb = child.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.isKinematic = true;
            }
        }
    }
}
