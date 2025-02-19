using UnityEngine;

public class ShaderMatrixSetter : MonoBehaviour
{
    public Material material;  // Reference to the material you want to modify

    void Update()
    {
        if (material != null && Camera.main != null)
        {
            // Pass the camera's view and projection matrices to the shader
            Camera camera = Camera.main;
            material.SetMatrix("_ViewMatrix", camera.worldToCameraMatrix);
            material.SetMatrix("_ProjectionMatrix", camera.projectionMatrix);
        }
    }
}
