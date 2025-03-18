using UnityEngine;
using Unity.Netcode;

public class bunsenBurnerScript : MonoBehaviour
{
    public float adjustmentSpeed = 0.3f;

    Transform gear;
    ParticleSystem flame;

    public bool isLit;

    // Networked variable for airflow (Server write permission)
    public float airflow = 0.2f;

    // Networked variable for gear's rotation angle
    public float gearRotation = -90f;

    void Start()
    {
        gear = transform.Find("Gear");
        flame = transform.Find("Flame").GetComponent<ParticleSystem>();
    }

    void Update()
    {

            // Update gear rotation based on airflow for the owner
            GearRotation();
            Airflow();

        // Apply the synchronized gear rotation to all clients
        gear.localEulerAngles = new Vector3(-90f, gearRotation, 0f);


        adjustFlameBasedOnAirFlow();
    }

    void adjustFlameBasedOnAirFlow()
    {
        var main = flame.main;
        var emission = flame.emission;

        // Flame Emission Rate
        emission.rateOverTime = 2500f * airflow + 500f;

        // Change flame speed
        main.startSpeed = 18f * airflow + 4f;

        // Change color
        main.startColor = Color.Lerp(new Color(0.5764706f, 0.1764706f, 0f), new Color(0f, 0.1176471f, 1f), airflow);
    }


    public void AdjustAirflow(float change)
    {
        airflow += change;
        airflow = Mathf.Clamp(airflow, 0f, 1f);  // Ensure the value stays in range
    }

    public void Airflow()
    {
        airflow = Mathf.Clamp(airflow, 0f, 1f); // Ensure airflow is within valid range
    }


    public void AdjustGearRotation(float rotationChange)
    {
        gearRotation += rotationChange;
    }


    public void GearRotation()
    {
        gearRotation = airflow * -360f;
    }

    // Method to adjust airflow based on input (called by players)
    public void AdjustAirflowBasedOnInput(float input)
    {

            AdjustAirflow(input * Time.deltaTime); // Send change to the server
    }

    // Use these methods to loosen or tighten gear based on user input
    public void loosenGear()
    {
            AdjustAirflow(Time.deltaTime * adjustmentSpeed);
            AdjustGearRotation(Time.deltaTime * adjustmentSpeed * -360f);
    }

    public void tightenGear()
    {

            AdjustAirflow(-Time.deltaTime * adjustmentSpeed);
            AdjustGearRotation(Time.deltaTime * adjustmentSpeed * 360f);
    }

}
