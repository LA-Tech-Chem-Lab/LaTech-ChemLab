using UnityEngine;

public class deleteAfterDelay : MonoBehaviour
{
    public float timeout = 1f;
    private float elapsed = 0f;
    
    void Update()
    {
        if (elapsed < timeout)
            elapsed += Time.deltaTime;
        else
            Destroy(gameObject);
    }
}
