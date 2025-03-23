using System.Collections;
using UnityEngine;

public class faucetHandleScript : MonoBehaviour
{
    public Transform hinge;
    public bool faucetIsOff = true;

    public float closedAngle = 0f;
    public float openAngle = 90f;
    public float blendingSensitivity = 3f;

    public AudioClip faucetStart;
    public AudioClip faucetLoop;

    private Coroutine faucetAudioCoroutine;
    private Quaternion targetQuaternion;

    void Start()
    {
        if (hinge == null)
        {
            hinge = transform.parent.gameObject.transform;
        }

        UpdateFaucetRotation(faucetIsOff);
    }

    void Update()
    {
        hinge.localRotation = Quaternion.Slerp(
            hinge.localRotation,
            targetQuaternion,
            Time.deltaTime * blendingSensitivity
        );
    }

    public void InteractWithThisFaucet()
    {
        faucetIsOff = !faucetIsOff; // Toggle faucet state
        UpdateFaucetRotation(faucetIsOff);

        if (!faucetIsOff)
        {
            // Start playing faucet sounds
            if (faucetAudioCoroutine != null)
            {
                StopCoroutine(faucetAudioCoroutine);
            }
            faucetAudioCoroutine = StartCoroutine(TurnOnFaucetAudio());
        }
        else
        {
            // Stop audio immediately
            if (faucetAudioCoroutine != null)
            {
                StopCoroutine(faucetAudioCoroutine);
                faucetAudioCoroutine = null;
            }
        }
    }

    private void UpdateFaucetRotation(bool isClosed)
    {
        targetQuaternion = Quaternion.Euler(0f, 0f, isClosed ? closedAngle : openAngle);
    }

    private IEnumerator TurnOnFaucetAudio()
    {
        // Play the start sound
        AudioSource.PlayClipAtPoint(faucetStart, transform.position);

        // Wait for faucetStart to finish, but stop if faucetIsOff becomes true
        float elapsedTime = 0f;
        while (elapsedTime < faucetStart.length)
        {
            if (faucetIsOff) yield break; // Stop immediately if faucet is turned off
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        // Ensure faucet is still on before starting the loop
        while (!faucetIsOff)
        {
            AudioSource.PlayClipAtPoint(faucetLoop, transform.position);
            float loopTime = faucetLoop.length;

            // Check every frame if faucet was turned off mid-loop
            elapsedTime = 0f;
            while (elapsedTime < loopTime)
            {
                if (faucetIsOff) yield break; // Stop immediately if faucet is turned off
                yield return null;
                elapsedTime += Time.deltaTime;
            }
        }
    }
}
