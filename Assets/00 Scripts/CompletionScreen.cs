using UnityEngine;
using UnityEngine.UI;
using System.Security.Cryptography;
using System.Text;
using System;
using TMPro;

public class CompletionScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject completionPanel;    
    public RawImage completionImage;         
    public TextMeshProUGUI completionCodeText;        // random code
    public TextMeshProUGUI completionTimeText;        // time completed
    public TextMeshProUGUI percentYieldText;
    public TextMeshProUGUI meltingPointText;

    [Header("Settings")]
    public int codeLength = 32;             // Length

    private string completionCode;
    private string completionTime;
    public float percentYield;
    public float meltingPoint;

    void Start()
    {
        // Hide panel at start
        if (completionPanel != null)
            completionPanel.SetActive(false);
    }

    // Call this function when it's time to show the completion screen
    public void ShowCompletionScreen()
    {
        if (completionPanel != null)
        {
            // Generate code and timestamp
            completionCode = GenerateRandomCode(codeLength);
            completionTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            completionPanel.SetActive(true);

            if (completionCodeText != null)
                completionCodeText.text = "Completion Code: " + completionCode;

            if (completionTimeText != null)
                completionTimeText.text = "Completed On: " + completionTime;

            if (percentYield != null){
                percentYieldText.text = "Percent Yield: " + percentYield + " %";
            }

            if (meltingPoint != null){
                meltingPointText.text = "Melting Point: " + meltingPoint + " C";
            }
        }
    }

    private string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        StringBuilder result = new StringBuilder(length);
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            byte[] uintBuffer = new byte[sizeof(uint)];

            while (result.Length < length)
            {
                rng.GetBytes(uintBuffer);
                uint num = BitConverter.ToUInt32(uintBuffer, 0);
                result.Append(chars[(int)(num % (uint)chars.Length)]);
            }
        }
        return result.ToString();
    }
}
