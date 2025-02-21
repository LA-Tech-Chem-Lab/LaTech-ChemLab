using Unity.VisualScripting;
using UnityEngine;

public class hoseScript : MonoBehaviour
{
    LineRenderer lineRenderer;

    public Vector3 angleFrom0To1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Set Anchor
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position + angleFrom0To1 * 0.2f);

    }
}
