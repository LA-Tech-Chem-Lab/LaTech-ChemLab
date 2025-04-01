using UnityEngine;
using UnityEngine.UI;

public class AlphaOscillatorUI : MonoBehaviour
{
    private Image filterImage;

    [Header("Oscillation Settings")]
    [Range(0f, 10f)]
    public float oscillationSpeed = 2f;

    [Range(0f, 1f)]
    public float minAlpha = 100f / 255f;

    [Range(0f, 1f)]
    public float maxAlpha = 1f;

    void Start()
    {
        filterImage = GetComponent<Image>();

        if (filterImage == null)
        {
            Debug.LogError("AlphaOscillatorUI: No Image component found on this GameObject!");
        }
    }

    void Update()
    {
        if (filterImage != null && gameObject.activeInHierarchy)
        {
            float t = (Mathf.Sin(Time.time * oscillationSpeed) + 1f) / 2f;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

            Color c = filterImage.color;
            c.a = alpha;
            filterImage.color = c;
        }
    }
}

