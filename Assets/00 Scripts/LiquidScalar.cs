using UnityEngine;

public class LiquidFillController : MonoBehaviour
{
    public Material liquidMaterial;  // The material of the liquid shader
    public Transform beakerTransform;  // The beaker object (or the parent object)

    private void Update()
    {
        // Get the tilt in the X or Z axis (for example)
        Vector3 rotation = beakerTransform.rotation.eulerAngles;

        // You could calculate the tilt using rotation around the X-axis, for example
        // Here we're mapping the rotation to a range of 0 to 1 for simplicity
        float tiltAmount = Mathf.Clamp01(Mathf.Abs(rotation.x) / 90f);  // Assuming a tilt range of 0 to 90 degrees

        // Pass the tilt information to the liquid shader
        liquidMaterial.SetFloat("_TiltAmount", tiltAmount);
    }
}
