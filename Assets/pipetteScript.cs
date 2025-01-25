using Obi;
using UnityEngine;
using UnityEngine.AI;

public class pipetteScript : MonoBehaviour
{
    public float checkTimeOut = 0.2f; float timeOfNextCheck;
    public float flowSpeed = 1f;
    public ObiEmitter emitter;
    public float pipetteMaxVollume = 100f;
    public float pipetteVollume;
    public bool pipetteFlowing;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeOfNextCheck = Time.time + checkTimeOut;
        emitter = transform.Find("Tip").GetComponent<ObiEmitter>();
        pipetteVollume = 100f;
        pipetteFlowing = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= timeOfNextCheck){

            timeOfNextCheck = Time.time + checkTimeOut;
            ObiSolver thissolver = emitter.solver;

            if (pipetteFlowing == true && pipetteVollume >= 0)
            {
                pipetteVollume -= 2.5f;
            }

            if (pipetteVollume <= 0f)
            {
                flowSpeed = 0f;
                pipetteVollume = 0f;
            }
            else{
                flowSpeed = 1f;
            }

            // foreach (int particleIndex in thissolver.activeParticles)
            // {
            //     Vector4 particlePosition = thissolver.positions[particleIndex];
            //     Vector3 worldPosition = emitter.solver.transform.TransformPoint(particlePosition);
            // }

        }

    }
}
