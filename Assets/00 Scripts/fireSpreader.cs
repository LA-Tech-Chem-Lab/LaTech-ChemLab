using UnityEngine;

public class FireSpreader : MonoBehaviour
{
    public GameObject firePrefab;        // The fire prefab that will spread and multiply
    public float spreadRadius = 2f;      // How far the fire can spread
    public float spreadInterval = 2f;    // How often the fire spreads
    public float lifespan = 10f;         // How long each fire prefab lasts before destroying itself
    public LayerMask spreadableLayer;    // Which layer the fire can spread on (e.g., Default, Floor)

    private bool isSpreading = false;

    void Start()
    {
        Debug.Log("newFire");
        Debug.Log(transform.position.x);
        // Start the fire lifecycle
        InvokeRepeating(nameof(SpreadFire), spreadInterval, spreadInterval);
        //Destroy(gameObject, lifespan); // Destroy itself after its lifespan
    }

    void SpreadFire()
    {
        Debug.Log("fire spreading");
        // Generate a random position within the spread radius
        Vector3 randomDirection = Random.insideUnitSphere * 0.2f;
        randomDirection.y = 0.1f; // Keep the fire on the same horizontal plane
        Vector3 spawnPosition = transform.position + randomDirection;

        // Spawn a new fire prefab at the randomly generated position
        Instantiate(firePrefab, spawnPosition, Quaternion.identity);
    }
}

