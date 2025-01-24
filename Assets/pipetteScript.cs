using Obi;
using UnityEngine;

public class pipetteScript : MonoBehaviour
{
    public float checkTimeOut = 0.2f; float timeOfNextCheck;
    public float flowSpeed = 3f;
    public ObiEmitter emitter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        timeOfNextCheck = Time.time + checkTimeOut;
        emitter = transform.Find("Tip").GetComponent<ObiEmitter>();
        Debug.Log(emitter.solver.activeParticles);
    }

    // Update is called once per frame
    void Update()
    {   
        if (Time.time >= timeOfNextCheck){
            
            timeOfNextCheck = Time.time + checkTimeOut;

            
            foreach (int particleIndex in emitter.solver.activeParticles)
            {
                Vector4 particlePosition = emitter.solver.positions[particleIndex];
                Vector3 worldPosition = emitter.solver.transform.TransformPoint(particlePosition);

                Debug.Log($"Particle {particleIndex} is at world position: {worldPosition}");
            }

        }

    }
}
