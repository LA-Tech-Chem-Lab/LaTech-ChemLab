using UnityEngine;

public class thrustFireExtinguisher : MonoBehaviour
{
    public float strength = 1f;
    public bool thrusting;
    public bool inSpace;
    public Rigidbody fireExtinguisherRB;
    
    public ParticleSystem foamPS;
    
    public floatInSpace spaceScript;

    ParticleSystem.MainModule main;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foamPS = GetComponent<ParticleSystem>();
        fireExtinguisherRB = transform.parent.GetComponent<Rigidbody>();
        spaceScript = transform.parent.GetComponent<floatInSpace>();
        
        main = foamPS.main;  // Get the MainModule
    }

    // Update is called once per frame
    void Update()
    {
        thrusting = foamPS.isPlaying;        
        inSpace = spaceScript.inSpace;

        if (thrusting)
            fireExtinguisherRB.AddForceAtPosition(transform.forward * strength, transform.position, ForceMode.Force);

        main.gravityModifier = inSpace ? 0f : 0.3f;
    }
}
