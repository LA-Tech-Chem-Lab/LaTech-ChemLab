using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

public class TemperatureCanvas : MonoBehaviour
{
    public GameObject canvasPanel; // The parent panel or canvas to enable/disable
    public GameObject TextPanel;
    public TextMeshProUGUI temperatureText; // Assign this in the Inspector

    private liquidScript liquid;
    private Camera playerCamera;
    float dotproudct;
    public GameObject beaker;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            // Get the camera from the player's child object
            playerCamera = player.transform.Find("Camera")?.GetComponent<Camera>();

            if (playerCamera == null)
            {
                Debug.LogWarning("Camera not found as a child of the player object.");
            }
        }
        else
        {
            Debug.LogWarning("Player object with tag 'Player' not found.");
        }
        // Try to find the Capillary Tube object by name
        GameObject capillaryTube = GameObject.Find("Capilary tube");
        beaker = capillaryTube.transform.parent?.parent?.parent?.gameObject;

        if (capillaryTube != null)
        {
            liquid = capillaryTube.GetComponent<liquidScript>();
            if (liquid == null)
            {
                Debug.LogWarning("Liquid script not found on Capillary Tube.");
            }
        }
        else
        {
            Debug.LogWarning("Capillary Tube object not found in the scene.");
        }

        if (temperatureText == null)
        {
            Debug.LogWarning("Temperature Text UI is not assigned.");
        }
    }

    void Update()
    {
        if (liquid != null && temperatureText != null)
        {
            float temperature = liquid.liquidTemperature;

            // Calculate vector from player camera to beaker
            Vector3 directionToBeaker = beaker.transform.position - playerCamera.transform.position;

            // Calculate dot product between camera forward vector and the direction to the beaker
            float dotProduct = Vector3.Dot(playerCamera.transform.forward, directionToBeaker.normalized);

            // Display temperature and state of matter
            temperatureText.text = temperature.ToString("F1") + " °C" + "\nState of Matter: Solid";


            // Enable the canvas panel if looking at the beaker (dot product > 0.7)
            if (dotProduct > 0.8f)
            {
                canvasPanel.SetActive(true); // Show the panel and text
                TextPanel.SetActive(true);
            }
            else
            {
                TextPanel.SetActive(false);
                canvasPanel.SetActive(false); // Hide the panel and text
            }
        }
    }
}