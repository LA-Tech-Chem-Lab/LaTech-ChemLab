using UnityEngine;
using UnityEngine.UI;

public class selectMicrophone : MonoBehaviour
{
    public Dropdown dropdown;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
        {

            #if UNITY_WEBGL && !UNITY_EDITOR
            dropdown.options.Add(new Dropdown.OptionData("Microphone not supported on WebGL"));
            #else
            foreach (var device in Microphone.devices)
            {
                dropdown.options.Add(new Dropdown.OptionData(device));
            }
            dropdown.onValueChanged.AddListener(ChangeMicrophone);
            
            var index = PlayerPrefs.GetInt("user-mic-device-index");
            dropdown.SetValueWithoutNotify(index);
            #endif
        }


    
    private void ChangeMicrophone(int index)
    {
        PlayerPrefs.SetInt("user-mic-device-index", index);
    }
}
