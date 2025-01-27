using Obi;
using UnityEngine;
using UnityEngine.AI;

public class pipetteScript : MonoBehaviour
{
    public float checkTimeOut = 0.2f; float timeOfNextCheck;
    public float flowSpeed = 1f;
    public ObiEmitter emitter;
    public float pipetteMaxVolume = 100f;
    public float pipetteVolume;
    public bool pipetteFlowing;
    public bool pipetteExtracting;
    float initialMaxVolume;

    liquidScript ls;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeOfNextCheck = Time.time + checkTimeOut;
        emitter = transform.Find("Tip").GetComponent<ObiEmitter>();
        pipetteVolume = 100f;
        initialMaxVolume = pipetteMaxVolume;
        pipetteFlowing = false;
        ls = GetComponent<liquidScript>();
        if (ls)
            ls.totalVolume_mL = pipetteMaxVolume;

    }

    // Update is called once per frame
    void Update()
    {
        pipetteVolume = Mathf.Clamp(pipetteVolume, 0f, pipetteMaxVolume);

        //if (pipetteFlowing == true && pipetteVolume >= 0)
        //{
        //    pipetteVolume -= 50f * Time.deltaTime;
        //}

        if (pipetteVolume <= 0f)
        {
            flowSpeed = 0f;
            pipetteVolume = 0f;
        }
        else{
            flowSpeed = 1f;
        }


        if (ls)
            ls.currentVolume_mL = pipetteVolume;

        if (Input.GetKeyDown(KeyCode.R))
            pipetteVolume = initialMaxVolume;
    }
}

        // foreach (int particleIndex in thissolver.activeParticles)
        // {
        //     Vector4 particlePosition = thissolver.positions[particleIndex];
        //     Vector3 worldPosition = emitter.solver.transform.TransformPoint(particlePosition);
        // }