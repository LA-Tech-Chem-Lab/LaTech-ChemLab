using UnityEngine;

public class LiquidController : MonoBehaviour
{
    public Material liquidMaterial; // Assign LiquidMaterial in the Inspector
    public float fillAmount = 0.5f; // Default fill level (0 to 1)

    void Update()
    {
        // Example: Increase fill with UP key, decrease with DOWN key
        if (Input.GetKey(KeyCode.UpArrow))
            fillAmount = Mathf.Clamp(fillAmount + Time.deltaTime * 0.2f, 0f, 1f);
        if (Input.GetKey(KeyCode.DownArrow))
            fillAmount = Mathf.Clamp(fillAmount - Time.deltaTime * 0.2f, 0f, 1f);

        // Update shader fill amount
        liquidMaterial.SetFloat("_FillAmount", fillAmount);
    }
}
