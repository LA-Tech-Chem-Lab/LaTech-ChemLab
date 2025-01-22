using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using TMPro;
using Palmmedia.ReportGenerator.Core;
using UnityEngine.Rendering;

public class multihandler : MonoBehaviour
{   
    public GameObject JoinCanvas;
    public GameObject InGameCanvas;
    public GameObject PauseCanvas;
    public GameObject Notebook;
    public GameObject SettingsPanel;
    public GameObject Finder;
    public GameObject Search;
    public GameObject microphoneSelectionDropdown;
    public bool isPaused;
    public bool notebookOpen;
    public bool searchOpen;
    public bool settingsOpen;
    public bool finderOpen;
    public TextMeshProUGUI helpText;
    private void Start()
    {
        JoinCanvas.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Subscribe to the client connection event
        NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnected;
    }

    List<float> timeOfEscapePresses = new List<float>(); float timeOutTime = 0.66f;
    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab)) && !JoinCanvas.activeInHierarchy) // We are loaded in and press escape
            PauseOrUnpause();

        // if (Input.GetKeyDown(KeyCode.Tab) && !isPaused)
        //     ToggleCursor();

        if (JoinCanvas.activeInHierarchy) { // Always show the mouse when selecting a server
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Quit game fast if you spam press Escape
        if (Input.GetKeyDown(KeyCode.BackQuote))
            timeOfEscapePresses.Add(Time.time);

        if (timeOfEscapePresses.Count > 0)
        {
            List<float> toRemove = new List<float>();
            foreach (float t in timeOfEscapePresses)
                if (Time.time - t > timeOutTime)
                    toRemove.Add(t);

            foreach (float t in toRemove)
                timeOfEscapePresses.Remove(t);
        }
        // Debug.Log(timeOfEscapePresses.Count);
        if (timeOfEscapePresses.Count > 3)
            QuitGame();

    }


    public void QuitGame()
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

    public void toggleNotebook(){
        notebookOpen = !notebookOpen;
        Notebook.SetActive(notebookOpen);
    }

    public void toggleSettings(){
        settingsOpen = !settingsOpen;
        SettingsPanel.SetActive(settingsOpen);
    }

    public void toggleFinder(){
        finderOpen = !finderOpen;
        Finder.SetActive(finderOpen);
    }

    public void toggleSearch(){
        searchOpen = !searchOpen;
        Search.SetActive(searchOpen);
    }

    public void PauseOrUnpause(){
        isPaused = !isPaused;

        if (isPaused){
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } else {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        PauseCanvas.SetActive(isPaused);
        InGameCanvas.SetActive(!isPaused);
    }

    public void setHelpText(string txt){
        helpText.text = txt;
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

    public void ClearAllServers()
    {
        Debug.Log("Clearing Servers");

        // Ensure that this method is only called by the server
        if (NetworkManager.Singleton.IsServer)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
            {
                Debug.Log($"Disconnecting client {client.ClientId}");
                NetworkManager.Singleton.DisconnectClient(client.ClientId);
            }
        }
        else
        {
            Debug.LogError("You must be the server to disconnect clients.");
        }
    }
}
