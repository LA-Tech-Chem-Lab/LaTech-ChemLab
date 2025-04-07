using System.Collections;
using UnityEngine;

public class faucetHandleScript : MonoBehaviour
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
            hinge = transform.parent.gameObject.transform;
        }

        UpdateFaucetRotation(faucetIsOff);
    }

    void Update()
    {
        hinge.localRotation = Quaternion.Slerp(
            hinge.localRotation,
            targetQuaternion,
            Time.deltaTime * blendingSensitivity
        );
    }

    public void InteractWithThisFaucet()
    {
        faucetIsOff = !faucetIsOff; // Toggle faucet state
        UpdateFaucetRotation(faucetIsOff);

    }

    private void UpdateFaucetRotation(bool isClosed)
    {
        targetQuaternion = Quaternion.Euler(0f, 0f, isClosed ? closedAngle : openAngle);
    }
}
