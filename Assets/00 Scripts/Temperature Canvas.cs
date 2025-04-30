using UnityEngine;
using TMPro;

public class TemperatureCanvas : MonoBehaviour
{
    public GameObject canvasPanel;
    public GameObject TextPanel;
    public TextMeshProUGUI temperatureText;

    private liquidScript liquid;
    private Camera playerCamera;
    public GameObject beaker;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerCamera = player.transform.Find("Camera")?.GetComponent<Camera>();
            if (playerCamera == null)
                Debug.LogWarning("Camera not found as a child of the player object.");
        }
        else
        {
            Debug.LogWarning("Player object with tag 'Player' not found.");
        }

        GameObject capillaryTube = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            if (obj.name == "Capilary tube")
            {
                float dist = Vector3.Distance(transform.position, obj.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    capillaryTube = obj;
                }
            }
        }

        if (capillaryTube != null)
        {
            beaker = capillaryTube.transform.parent?.parent?.parent?.gameObject;
            liquid = capillaryTube.GetComponent<liquidScript>();

            if (liquid == null)
                Debug.LogWarning("Liquid script not found on Capillary Tube.");
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
        if (liquid != null && temperatureText != null && playerCamera != null && beaker != null)
        {
            if (liquid.currentVolume_mL <= 0f)
            {
                temperatureText.text = "Capillary Tube: empty";
            }
            else
            {
                float temperature = liquid.liquidTemperature - 273.15f;
                Vector3 directionToBeaker = beaker.transform.position - playerCamera.transform.position;
                float dotProduct = Vector3.Dot(playerCamera.transform.forward, directionToBeaker.normalized);

                float meltingPoint = liquid.GetMeltingPoint();
                string state = liquid.liquidTemperature < meltingPoint ? "Solid" : "Liquid";
                temperatureText.text = $"{temperature:F1} °C\nState of Matter: {state}";

                bool isLookingAtBeaker = dotProduct > 0.8f;
                canvasPanel.SetActive(isLookingAtBeaker);
                TextPanel.SetActive(isLookingAtBeaker);
            }
        }
    }
}
