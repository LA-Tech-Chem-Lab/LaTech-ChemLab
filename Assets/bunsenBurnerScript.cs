using UnityEngine;

public class bunsenBurnerScript : MonoBehaviour
{
    public float airflow = 0.2f; // ranges from 0-1
    public float adjustmentSpeed = 0.3f;

    public Transform gear;

    void Start()
    {
        gear = transform.Find("Gear");
    }

    void Update()
    {
        gear.localEulerAngles = new Vector3(-90f, airflow * 360f, 0f);
        airflow = Mathf.Clamp(airflow, 0f, 1f);
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
}
