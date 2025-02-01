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
        Debug.Log("Tare Update Value" + tareTracker.Value);
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
            impulsePerRigidBody[collision.rigidbody] = collision.impulse.y / lastDeltaTime;
            UpdateWeight();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.rigidbody != null)
        {
            impulsePerRigidBody[collision.rigidbody] = collision.impulse.y / lastDeltaTime;
            UpdateWeight();
        }
    }


    private void OnCollisionExit(Collision collision)
    {
        if (collision.rigidbody != null)
        {
            impulsePerRigidBody.Remove(collision.rigidbody);
            UpdateWeight();
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
            Debug.Log("Tare newmass Value" + tareTracker.Value);

            // Update the calculated mass on the server
            calculatedMass.Value = newMass;

            // Broadcast the update to all clients
            UpdateMassClientRpc(newMass);

            // Also update the server's UI text (if necessary)
            UpdateMassText(newMass);
    }


    [ServerRpc(RequireOwnership = false)]
    public void RequestTareServerRpc()
    {

        tareTracker.Value = calculatedMass.Value;
        Debug.Log("Tare Value after Reassigned" + tareTracker.Value);
        UpdateWeight();
    }

    public void Tare()
    {
        if (IsClient)
        {
            Debug.Log("Client trying to tare");
            RequestTareServerRpc();
        }
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
