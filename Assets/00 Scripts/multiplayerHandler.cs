using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;

public class multiplayerHandler : MonoBehaviour
{
    public GameObject JoinCanvas;
    public GameObject InGameCanvas;

    private void Start()
    {
        JoinCanvas.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Subscribe to the client connection event
        NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnected;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            QuitGame();

        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleCursor();

        if (JoinCanvas.activeInHierarchy) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerConnected;
        }
    }

    private void OnPlayerConnected(ulong clientId)
    {
        JoinCanvas.SetActive(false);
        InGameCanvas.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void QuitGame()
    {

        Application.Quit();

        #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
        #endif
    }

    void ToggleCursor()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

}