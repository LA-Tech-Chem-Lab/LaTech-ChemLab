using UnityEngine;

public class IceBath : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LiquidHolder"))  
        {
            liquidScript liquid = other.GetComponent<liquidScript>();
            if (liquid != null)
            {
                // Start the cooling effect
                liquid.isinIceBath = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LiquidHolder"))
        {
            liquidScript liquidScript = other.GetComponent<liquidScript>();
            if (liquidScript != null)
            {
                // Stop cooling if the liquid leaves the ice bath
                liquidScript.isinIceBath = false;
            }
        }
    }
}
