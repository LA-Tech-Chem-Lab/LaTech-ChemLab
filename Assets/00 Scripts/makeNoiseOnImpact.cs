using UnityEngine;

public class makeNoiseOnImpact : MonoBehaviour
{   
    
    public float forceThreshold = 100f; // Define a force limit for shattering if needed
    public AudioClip Sound;
    public float VolumeScale = 1;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        float impactForce = collision.impulse.magnitude / Time.fixedDeltaTime / rb.mass;
        Debug.Log("Impact Force: " + impactForce);

        if (impactForce > forceThreshold)
            playSound();
    }


    void playSound(){
        GameObject tempAudio = new GameObject("TempAudio");
        AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
        audioSource.clip = Sound;
        audioSource.volume = VolumeScale;
        audioSource.Play();
        Destroy(tempAudio, Sound.length); // Cleanup
    }
}