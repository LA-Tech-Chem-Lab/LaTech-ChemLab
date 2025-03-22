using UnityEngine;

public class shatterGlassScript : MonoBehaviour
{
    public float forceLimit = 300; // Define a force limit for shattering if needed
    public GameObject brokenVersionOfGlass;
    public AudioClip dinkSound;
    public float dinkVolumeScale;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        float impactForce = collision.impulse.magnitude / Time.fixedDeltaTime / rb.mass;
        Debug.Log("Impact Force: " + impactForce);

        if (impactForce >= forceLimit)
            breakGlass();
        else
            playGlassDinkSound(impactForce / forceLimit);
    }

    void breakGlass(){
        Instantiate(brokenVersionOfGlass, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    void playGlassDinkSound(float scale){
        GameObject tempAudio = new GameObject("TempAudio");
        AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
        audioSource.clip = dinkSound;
        audioSource.volume = scale * dinkVolumeScale;
        audioSource.Play();
        Destroy(tempAudio, dinkSound.length); // Cleanup
    }
}