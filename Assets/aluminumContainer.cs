using UnityEngine;

public class AluminumContainer : MonoBehaviour
{
    public Transform aluminumContents;

    float dot;
    public float value = 0f; // The float we want to track
    float velocity = 0f; // The rate of change of 'value'
    float minValue = 0;
    float maxValue = 0.2211f;

    void Start()
    {
        aluminumContents = transform.Find("Aluminum");
    }

    void Update()
    {
        simulateGravity();
        aluminumContents.transform.localPosition = new Vector3(0f, value, 0f);
    }

    void simulateGravity(){
        dot = Vector3.Dot(transform.up, Vector3.up);
        if (Mathf.Abs(dot) > 0.2f)
            dot = Mathf.Sign(dot);

        float acceleration = -24f * dot;

        if ((value > minValue || acceleration > 0) && (value < maxValue || acceleration < 0))
            velocity += acceleration * Time.deltaTime;
        else
            velocity = 0f;

        value += velocity * Time.deltaTime;
        value = Mathf.Clamp(value, minValue, maxValue);
    }
}
