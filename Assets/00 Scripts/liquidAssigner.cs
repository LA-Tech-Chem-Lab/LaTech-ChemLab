using UnityEngine;

public class MaterialAssigner : MonoBehaviour
{
    public Material newMaterial; // Drag and drop your Material in the Inspector

    void Start()
    {
        if (newMaterial != null)
        {
            Renderer objectRenderer = GetComponent<Renderer>();

            if (objectRenderer != null)
            {
                objectRenderer.material = newMaterial; // Assign the material
            }
            else
            {
                Debug.LogWarning("No Renderer found on this GameObject.");
            }
        }
        else
        {
            Debug.LogWarning("No Material assigned in the Inspector.");
        }
    }
}

