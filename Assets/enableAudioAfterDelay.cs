using System.Threading;
using UnityEngine;

public class enableAudioAfterDelay : MonoBehaviour
{

    AudioListener al;
    public float secondsOfSilence = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        al = GetComponent<AudioListener>();
        al.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeSinceLevelLoad > secondsOfSilence && !al.enabled)
            al.enabled = true;

    }
}
