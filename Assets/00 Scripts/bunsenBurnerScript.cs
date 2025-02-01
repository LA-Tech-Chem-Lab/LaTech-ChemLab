using UnityEngine;
using Unity.Netcode;

public class bunsenBurnerScript : NetworkBehaviour
{
    public float adjustmentSpeed = 0.3f;

    Transform gear;
    ParticleSystem flame;

    Color redFlame, blueFlame;

    // Networked variable for airflow (Server write permission)
    public NetworkVariable<float> airflow = new NetworkVariable<float>(0.2f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // Default airflow

    // Networked variable for gear's rotation angle
    public NetworkVariable<float> gearRotation = new NetworkVariable<float>(-90f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Start()
    {
        gear = transform.Find("Gear");
        flame = transform.Find("Flame").GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (IsOwner)  // Ensures only the owner can change airflow locally
        {
            // Update gear rotation based on airflow for the owner
            GearRotationServerRpc();
            AirflowServerRpc();
        }

        // Apply the synchronized gear rotation to all clients
        gear.localEulerAngles = new Vector3(-90f, gearRotation.Value, 0f);


        adjustFlameBasedOnAirFlow();
    }

    void adjustFlameBasedOnAirFlow()
    {
        var main = flame.main;
        var emission = flame.emission;

        // Flame Emission Rate
        emission.rateOverTime = 2500f * airflow.Value + 500f;

        // Change flame speed
        main.startSpeed = 18f * airflow.Value + 4f;

        // Change color
        main.startColor = Color.Lerp(new Color(0.5764706f, 0.1764706f, 0f), new Color(0f, 0.1176471f, 1f), airflow.Value);
    }

    // ServerRpc to adjust airflow
    [ServerRpc]
    public void AdjustAirflowServerRpc(float change)
    {
        airflow.Value += change;
        airflow.Value = Mathf.Clamp(airflow.Value, 0f, 1f);  // Ensure the value stays in range
    }

    [ServerRpc]
    public void AirflowServerRpc()
    {
        airflow.Value = Mathf.Clamp(airflow.Value, 0f, 1f); // Ensure airflow is within valid range
    }

    // ServerRpc to adjust the gear rotation
    [ServerRpc]
    public void AdjustGearRotationServerRpc(float rotationChange)
    {
        gearRotation.Value += rotationChange;
    }

    // ServerRpc to adjust the gear rotation
    [ServerRpc]
    public void GearRotationServerRpc()
    {
        gearRotation.Value = airflow.Value * -360f;
    }

    // Method to adjust airflow based on input (called by players)
    public void AdjustAirflowBasedOnInput(float input)
    {
        if (IsOwner)
        {
            AdjustAirflowServerRpc(input * Time.deltaTime); // Send change to the server
        }
    }

    // Use these methods to loosen or tighten gear based on user input
    public void loosenGear()
    {
        if (IsOwner)  // Ensure only the local player can change their gear
        {
            AdjustAirflowServerRpc(Time.deltaTime * adjustmentSpeed);
            AdjustGearRotationServerRpc(Time.deltaTime * adjustmentSpeed * -360f);
        }
    }

    public void tightenGear()
    {
        if (IsOwner)  // Ensure only the local player can change their gear
        {
            AdjustAirflowServerRpc(-Time.deltaTime * adjustmentSpeed);
            AdjustGearRotationServerRpc(Time.deltaTime * adjustmentSpeed * 360f);
        }
    }

    // For parsing hex colors
    Color colorFromHex(string hex)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color))
        {
            return color;
        }
        else
        {
            Debug.LogError("Invalid hex color string!");
            return Color.white;  // Default fallback color if parsing fails
        }
    }
}
