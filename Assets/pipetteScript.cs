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

    }

    // Update is called once per frame
    void Update()
    {   
        if (Time.time >= timeOfNextCheck){
            
            timeOfNextCheck = Time.time + checkTimeOut;
            ObiSolver thissolver = emitter.solver;
            
            foreach (int particleIndex in thissolver.activeParticles)
            {
                Vector4 particlePosition = thissolver.positions[particleIndex];
                Vector3 worldPosition = emitter.solver.transform.TransformPoint(particlePosition);

                if (worldPosition.y <= 0.2f) emitter.solver.life[particleIndex] = 0f;
            }

        }

    }
}
