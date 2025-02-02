using UnityEngine;

public class shiftBy : MonoBehaviour
{   
    public Vector3 displacement;
    public Vector3 targetPosOffset;
    public float checkRadiusOverride;

    public Vector3 GetOffset(){
        return displacement;
    }

    public Vector3 GetTargetPosOffset(){
        return targetPosOffset;
    }
}
