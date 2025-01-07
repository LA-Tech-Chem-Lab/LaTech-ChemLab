using UnityEngine;
using TMPro;

public class scalecontroller : MonoBehaviour
{
    public TextMeshProUGUI massText; 
    private float totalMass = 0f;

    void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.rigidbody;
        if (rb != null)
        {   
            Debug.Log(rb.gameObject.name);
            totalMass += rb.mass; 
            UpdateMassDisplay();
        }
    }

    void OnCollisionExit(Collision collision)
    {
        Rigidbody rb = collision.rigidbody;
        if (rb != null)
        {
            totalMass -= rb.mass; 
            UpdateMassDisplay();
        }
    }

    void UpdateMassDisplay()
    {
        massText.text = totalMass.ToString("F2") + " kg"; 
    }
}

