
using UnityEngine;

public class makeNoiseOnImpact : MonoBehaviour
{   
    
    public float soundThreshold = 100f; // Define a force limit for shattering if needed
    public float breakThreshold;
    public GameObject brokenVersionOfObject;
    public float OverwriteScaleThresh;
    public AudioClip Sound;
    public float VolumeScale = 1f;
    public float PitchScale = 1f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        float impactForce = collision.impulse.magnitude / Time.fixedDeltaTime / rb.mass;
        Debug.Log("Impact Force: " + impactForce);

        if (impactForce > breakThreshold && breakThreshold > soundThreshold){
            breakObject();
            return;
        }    
        
        if (OverwriteScaleThresh > 0f){
            playScaledSound(impactForce / soundThreshold);
        }
        
        else if (impactForce > soundThreshold)
            playSound();
    }

    void breakObject(){
        if (brokenVersionOfObject){
            Instantiate(brokenVersionOfObject, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }



    void playSound(){
        GameObject tempAudio = new GameObject("TempAudio");
        AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
        audioSource.clip = Sound;
        audioSource.volume = VolumeScale;
        audioSource.pitch = PitchScale; // Adjusts speed and pitch
        audioSource.Play();
        Destroy(tempAudio, Sound.length / PitchScale); // Adjust cleanup time based on speed
    }

    void playScaledSound(float givenScale){
        GameObject tempAudio = new GameObject("TempAudio");
        AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
        audioSource.clip = Sound;
        audioSource.volume = VolumeScale * Mathf.Clamp(givenScale, 0f, 1f);
        audioSource.pitch = PitchScale; // Adjusts speed and pitch
        audioSource.Play();
        Destroy(tempAudio, Sound.length); // Cleanup
    }
}