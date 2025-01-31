using UnityEngine;

public class bunsenBurnerScript : MonoBehaviour
{
    public float airflow = 0.2f; // ranges from 0-1
    public float adjustmentSpeed = 0.3f;

    Transform gear;
    ParticleSystem flame;

    Color redFlame; Color blueFlame;

    void Start()
    {
        gear = transform.Find("Gear");
        flame = transform.Find("Flame").GetComponent<ParticleSystem>();
        
    }

    void Update()
    {
        gear.localEulerAngles = new Vector3(-90f, airflow * -360f, 0f);
        airflow = Mathf.Clamp(airflow, 0f, 1f);

        adjustFlameBasedOnAirFlow();
    }

    void adjustFlameBasedOnAirFlow(){
        var main = flame.main;
        var emission = flame.emission;

        // Flame Emission Rate
        emission.rateOverTime = 2500f * airflow + 500f;

        // Change flame speed
        main.startSpeed = 18f * airflow + 4f;

        // Change color
        main.startColor = Color.Lerp(new Color(0.5764706f, 0.1764706f, 0f), new Color(0f, 0.1176471f, 1f), airflow);
    }









    public void loosenGear(){
        airflow += Time.deltaTime * adjustmentSpeed;
    }

    public void tightenGear(){
        airflow -= Time.deltaTime * adjustmentSpeed;
    }

    public void adjustGearBasedOnInput(float input){
        airflow += Time.deltaTime * input;
    }

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
