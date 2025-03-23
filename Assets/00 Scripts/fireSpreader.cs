using UnityEngine;
using System.Collections;

public class FireSpread : MonoBehaviour
{
    public GameObject firePrefab;        // The fire prefab that will spread and multiply (should be itself)
    public float spreadRadius = 0.2f;      // How far the fire spreads each time
    public float spreadInterval = 2f;    // How often the fire spreads (in seconds)
    public float lifespan = 5f;          // How long each fire prefab lasts before destroying itself
    public int maxFires = 50;            // Maximum number of fires allowed in the scene
    public float flameGrowthRate = 1.01f; // Multiplier for how much bigger each generation of flames gets
    public int generation = 0;           // Tracks how deep in the spreading chain this fire is

    private static int currentFireCount = 0; // Tracks how many fires are currently active
    private static bool collisionIgnored = false;

    void Start()
    {
        // Register the newly spawned fire
        currentFireCount++;

        if (!collisionIgnored)
        {
            int fireLayer = LayerMask.NameToLayer("Fire");
            Physics.IgnoreLayerCollision(fireLayer, fireLayer, true);
            collisionIgnored = true; // Prevent this from running more than once
        }
        
        // Start the spreading cycle
        StartCoroutine(SpreadFire());

        // Destroy itself after its lifespan
        //Destroy(gameObject, lifespan);
    }

    IEnumerator SpreadFire()
    {   
        for (int i = 0; i < 50; i++)
        {
            yield return new WaitForSeconds(4f);

            //if (currentFireCount <= maxFires) // Prevent overpopulation of fires
            //{
                Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * spreadRadius;
                Vector3 randomDirection = new Vector3(randomCircle.x, 0, randomCircle.y);
                Vector3 spawnPosition = transform.position + randomDirection;

                // Instantiate a new fire prefab at the calculated position
                GameObject newFire = Instantiate(firePrefab, spawnPosition, Quaternion.identity);
                
                // Increase the generation count for the newly spawned fire
                FireSpread newFireSpread = newFire.GetComponent<FireSpread>();
                newFireSpread.generation = generation + 1; // Increase generation count

                // Make the fire grow in size as it spreads
                ParticleSystem flame = newFire.GetComponent<ParticleSystem>();
                if (flame != null)
                {
                    var main = flame.main;
                    main.startSizeMultiplier *= Mathf.Pow(flameGrowthRate, newFireSpread.generation); // Increase size based on generation
                    flame.Play();
                }

                // Destroy the fire after its lifespan
                Destroy(newFire, lifespan);

                // Register the newly spawned fire
                //currentFireCount++;
            //}
        }
    }

    private void OnDestroy()
    {
        // Decrement the fire count when this fire is destroyed
        currentFireCount--;
    }
}



