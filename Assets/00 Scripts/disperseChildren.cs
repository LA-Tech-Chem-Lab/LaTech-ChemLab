using UnityEngine;

public class disperseChildren : MonoBehaviour
{
    void Start()
    {
        // Unparent all children
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--) // Loop backwards to avoid indexing issues
        {
            Transform child = transform.GetChild(i);
            child.SetParent(null); // Move to the root of the hierarchy
        }

        // Destroy this GameObject
        Destroy(gameObject);
    }
}
