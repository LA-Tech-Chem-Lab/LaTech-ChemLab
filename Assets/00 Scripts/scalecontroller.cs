using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;

public class WeightScale : NetworkBehaviour
{
    float forceToMass;
    public TextMeshProUGUI massText;

    private Dictionary<Rigidbody, float> impulsePerRigidBody = new Dictionary<Rigidbody, float>();

    private float currentDeltaTime;
    private float lastDeltaTime;


    private NetworkVariable<float> tareTracker = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Network variable for mass
    private NetworkVariable<float> calculatedMass = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        forceToMass = 1f / Physics.gravity.magnitude;
    }

    private void Start()
    {
        if (IsClient)
        {
            calculatedMass.OnValueChanged += (oldValue, newValue) =>
            {
                UpdateMassText(newValue);
            };
        }
    }

    private void FixedUpdate()
    {
        lastDeltaTime = currentDeltaTime;
        currentDeltaTime = Time.fixedDeltaTime;

    }

    private void UpdateWeight()
    {

        float combinedForce = 0;
        foreach (var force in impulsePerRigidBody.Values)
        {
            combinedForce += force;
        }

        float newMass = (combinedForce * forceToMass) - tareTracker.Value;
        if (IsClient)
        {
            RequestWeightVariableUpdateServerRpc();
            UpdateMassServerRpc(newMass); // Ensure the server gets the updated mass
        }
        
        if (IsServer)
        {
            calculatedMass.Value = newMass;
        }
    }

    private void UpdateMassText(float mass)
    {
        massText.text = (mass * 1000f).ToString("F2") + " g";
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody != null)
        {
            float impulseValue = collision.impulse.y / lastDeltaTime;
            impulsePerRigidBody[collision.rigidbody] = impulseValue;
            UpdateWeight();

            // Get NetworkObject and send data to the server
            if (collision.rigidbody.TryGetComponent(out NetworkObject netObj))
            {
                SendImpulseToServerRpc(netObj, impulseValue); // Pass NetworkObjectReference
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.rigidbody != null)
        {
            float impulseValue = collision.impulse.y / lastDeltaTime;
            impulsePerRigidBody[collision.rigidbody] = impulseValue;
            UpdateWeight();

            // Get NetworkObject and send data to the server
            if (collision.rigidbody.TryGetComponent(out NetworkObject netObj))
            {
                SendImpulseToServerRpc(netObj, impulseValue); // Pass NetworkObjectReference
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.rigidbody != null)
        {
            // Remove from local dictionary
            impulsePerRigidBody.Remove(collision.rigidbody);
            UpdateWeight();

            // Get NetworkObject and notify the server
            if (collision.rigidbody.TryGetComponent(out NetworkObject netObj))
            {
                RemoveImpulseFromServerRpc(netObj); // Pass NetworkObjectReference
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestWeightVariableUpdateServerRpc()
    {
            // Server calculates the mass
            float combinedForce = 0;
            foreach (var force in impulsePerRigidBody.Values)
            {
                combinedForce += force;
            }

            float newMass = (combinedForce * forceToMass) - tareTracker.Value;

            // Update the calculated mass on the server
            calculatedMass.Value = newMass;

            // Broadcast the update to all clients
            UpdateMassClientRpc(newMass);

            // Also update the server's UI text (if necessary)
            UpdateMassText(newMass);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendImpulseToServerRpc(NetworkObjectReference objectRef, float impulseValue)
    {
        if (objectRef.TryGet(out NetworkObject netObj))
        {
            Rigidbody rb = netObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                impulsePerRigidBody[rb] = impulseValue;
                UpdateWeight();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveImpulseFromServerRpc(NetworkObjectReference objectRef)
    {
        if (objectRef.TryGet(out NetworkObject netObj))
        {
            Rigidbody rb = netObj.GetComponent<Rigidbody>();
            if (rb != null && impulsePerRigidBody.ContainsKey(rb))
            {
                impulsePerRigidBody.Remove(rb);
                UpdateWeight();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestTareServerRpc()
    {
        if (IsServer)
        {
            // Server calculates the mass
            float combinedForce = 0;
            foreach (var force in impulsePerRigidBody.Values)
            {
                combinedForce += force;
            }
            tareTracker.Value = (combinedForce * forceToMass);
            UpdateWeight();

        }
        else
        {
            RequestTareClientRpc();
        }
    }

    [ClientRpc]
    public void RequestTareClientRpc()
    {
        if (!IsServer)
        {
            // Server calculates the mass
            float combinedForce = 0;
            foreach (var force in impulsePerRigidBody.Values)
            {
                combinedForce += force;
            }
            tareTracker.Value = (combinedForce * forceToMass);
            UpdateWeight();
        }
    }

    public void Tare()
    {
            RequestTareServerRpc();
    }

    [ClientRpc]
    private void UpdateMassClientRpc(float newMass)
    {
        if (!IsServer) // Prevent host from calling this unnecessarily
        {
            UpdateMassText(newMass);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateMassServerRpc(float newMass)
    {
            UpdateMassText(newMass);
            UpdateMassClientRpc(newMass);
    }
}
