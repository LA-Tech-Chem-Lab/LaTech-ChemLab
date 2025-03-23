using UnityEngine;
using System.Collections;

public class FireSpread : MonoBehaviour
{
    public GameObject firePrefab;        // The fire prefab that will spread and multiply (should be itself)
    public float spreadRadius = 2f;      // How far the fire spreads each time
    public float spreadInterval = 2f;    // How often the fire spreads (in seconds)
    public float lifespan = 10f;         // How long each fire prefab lasts before destroying itself
    public int maxFires = 50;            // Maximum number of fires allowed in the scene

    private static int currentFireCount = 0; // Tracks how many fires are currently active

    void Start()
    {
        // Register the newly spawned fire
        currentFireCount++;
        
        // Start the spreading cycle
        StartCoroutine(SpreadFire());

        // Destroy itself after its lifespan
    }

    IEnumerator SpreadFire()
    {   
        for (int i = 0; i < 5; i++){
            if (currentFireCount <= 100){ // Prevent overpopulation of fires

                Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * 0.2f;
                Vector3 randomDirection = new Vector3(randomCircle.x, 0, randomCircle.y);
                Vector3 spawnPosition = transform.position + randomDirection;

                // Instantiate a new fire prefab at the calculated position
                GameObject newFire = Instantiate(firePrefab, spawnPosition, Quaternion.identity);
                ParticleSystem flame = newFire.GetComponent<ParticleSystem>();
                Destroy(newFire, lifespan);
                flame.Play();

                // Register the newly spawned fire
                currentFireCount++;
            }
            yield return new WaitForSeconds(4f);
        }
    }

    private void OnDestroy()
    {
        // Decrement the fire count when this fire is destroyed
        currentFireCount--;
    }
}


