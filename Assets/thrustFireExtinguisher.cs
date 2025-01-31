using UnityEngine;

public class thrustFireExtinguisher : MonoBehaviour
{
    public float strength = 1f;
    public bool thrusting;
    public Rigidbody fireExtinguisherRB;
    
    public ParticleSystem foamPS;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foamPS = GetComponent<ParticleSystem>();
        fireExtinguisherRB = transform.parent.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        thrusting = foamPS.isPlaying;

        if (thrusting)
            fireExtinguisherRB.AddForceAtPosition(transform.forward * strength, transform.position, ForceMode.Force);
    }
}
