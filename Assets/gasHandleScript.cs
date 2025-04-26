using System.Collections;
using UnityEngine;

public class gasHandleScript : MonoBehaviour
{
    public Transform hinge;
    public bool faucetIsOff = true;

    public float closedAngle = 0f;
    public float openAngle = 90f;
    public float blendingSensitivity = 3f;
    
    private Quaternion targetQuaternion;

    void Start()
    {
        if (hinge == null)
        {
            hinge = transform;
        }

        UpdateValveRotation(faucetIsOff);
    }

    void Update()
    {
        hinge.localRotation = Quaternion.Slerp(
            hinge.localRotation,
            targetQuaternion,
            Time.deltaTime * blendingSensitivity
        );
    }

    public void InteractWithThisValve()
    {
        faucetIsOff = !faucetIsOff; // Toggle faucet state
        UpdateValveRotation(faucetIsOff);

    }

    private void UpdateValveRotation(bool isClosed)
    {
        targetQuaternion = Quaternion.Euler(90f, 0f, isClosed ? closedAngle : openAngle);
    }
}
