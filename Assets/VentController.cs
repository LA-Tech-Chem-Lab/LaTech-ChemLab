using UnityEngine;

public class VentController : MonoBehaviour
{
    public bool vacuumOn; doorScriptXAxis HandleScript;
    Transform FIRST_JOINT;
    Transform SECOND_JOINT;
    Transform THIRD_JOINT;
    Transform FOURTH_JOINT;
    public float FirstJointY;
    public float SecondJointX;
    public float ThirdJointX;
    public float FourthJointX;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FIRST_JOINT = transform.Find("FIRST JOINT");
        SECOND_JOINT = FIRST_JOINT.Find("SECOND JOINT");
        THIRD_JOINT = SECOND_JOINT.Find("THIRD JOINT");
        FOURTH_JOINT = THIRD_JOINT.Find("FOURTH JOINT");
        HandleScript = THIRD_JOINT.Find("4to5").Find("Handle Hinge").Find("Handle").GetComponent<doorScriptXAxis>();
    }

    // Update is called once per frame
    void Update()
    {   
        vacuumOn = !HandleScript.doorIsClosed;
        // Constrain Angles
        applyAngles();
    }

    void applyAngles(){
        FIRST_JOINT.localEulerAngles = new Vector3(0f, FirstJointY, 0f);
        SECOND_JOINT.localEulerAngles = new Vector3(SecondJointX, 0f, 0f);
        THIRD_JOINT.localEulerAngles = new Vector3(ThirdJointX, 0f, 0f);
        FOURTH_JOINT.localEulerAngles = new Vector3(FourthJointX, 0f, 0f);
    }
}
