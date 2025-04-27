using UnityEngine;

public class shatterGlassScript : MonoBehaviour
{
    public float forceLimit = 400f; // Define a force limit for shattering if needed
    public GameObject brokenVersionOfGlass;
    public AudioClip dinkSound;
    public float dinkVolumeScale = 1f;
    public float dinkPitchScale = 1f;
    public bool scaleAudioFromSurvivingFall = true;
    pickUpObjects pickUpScript;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pickUpScript = FindObjectOfType<pickUpObjects>();
    }

    void OnCollisionEnter(Collision collision)
    {
        float impactForce = collision.impulse.magnitude / Time.fixedDeltaTime / rb.mass;
        Debug.Log("Impact Force: " + impactForce);

        if (impactForce >= forceLimit && brokenVersionOfGlass)
            breakGlass();
        else
            if (scaleAudioFromSurvivingFall)
                playGlassDinkSound(impactForce / forceLimit);
            else
                playGlassDinkSound(1f);
    }

    void breakGlass(){
        pickUpScript.DropItem();
        Instantiate(brokenVersionOfGlass, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    void playGlassDinkSound(float scale){
        GameObject tempAudio = new GameObject("TempAudio");
        AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
        audioSource.clip = dinkSound;
        audioSource.volume = scale * dinkVolumeScale;
        audioSource.pitch = dinkPitchScale; // Adjusts speed and pitch
        audioSource.Play();
        Destroy(tempAudio, dinkSound.length); // Cleanup
    }
}