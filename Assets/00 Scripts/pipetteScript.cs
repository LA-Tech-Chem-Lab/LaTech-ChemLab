using System.Collections.Generic;
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
    public string liquidType;
    public List<float> pipetteSolution = new List<float> {0f, 0f, 0f};

    liquidScript ls;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeOfNextCheck = Time.time + checkTimeOut;
        emitter = transform.Find("Tip").GetComponent<ObiEmitter>();
        pipetteVolume = 0f;
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

        //sets the flow speed of the pipette according to whether or not there is liquid in it
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

        //fills the pipette on click of R 
        if (Input.GetKeyDown(KeyCode.R))
            pipetteVolume = initialMaxVolume;
    }
}
