using UnityEngine;

public class shatterGlassScript : MonoBehaviour
{
    public float forceLimit = 300; // Define a force limit for shattering if needed
    public GameObject brokenVersionOfGlass;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        float impactForce = collision.impulse.magnitude / Time.fixedDeltaTime;
        Debug.Log("Impact Force: " + impactForce);

        if (impactForce > forceLimit)
            breakGlass();
    }

    void breakGlass(){
        Instantiate(brokenVersionOfGlass, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}