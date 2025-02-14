using UnityEngine;
using UnityEngine.UI;

public class Kill : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(Quit);
    }
    void Quit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in the editor
        #else
            Application.Quit(); // Quit the game in a built version
        #endif
    }
}
