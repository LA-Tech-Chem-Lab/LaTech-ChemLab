using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using TMPro;
using UnityEngine.Rendering;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample;
using System.Text.RegularExpressions;
using UnityEngine.UI;

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

    [Header("Text Chat")]
    public bool isTyping;
    public GameObject chatCanvas;

    public TMP_InputField chatInputField;
    public TextMeshProUGUI chatText;
    

    private void Start()
    {
        JoinCanvas.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Subscribe to the client connection event
        NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnected;

        chatInputField = chatCanvas.GetComponent<TMP_InputField>();
    }

    void Update()
    {   
        if (Input.GetKeyDown(KeyCode.Escape) && JoinCanvas.activeInHierarchy) // We are selecting server and press escape - Quit
            QuitGame();

        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab)) && !JoinCanvas.activeInHierarchy) // We are loaded in and press escape
            PauseOrUnpause();

        if (Input.GetKeyDown(KeyCode.Return) && !JoinCanvas.activeInHierarchy) // We press enter ONLY when we are in game
            StartOrStopTyping();
        // if (Input.GetKeyDown(KeyCode.Tab) && !isPaused)
        //     ToggleCursor();

        if (JoinCanvas.activeInHierarchy) { // Always show the mouse when selecting a server
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        if (isTyping)
            getTextChatFromInput();
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

        // isTyping = false;
    }


    public void setHelpText(string txt){
        helpText.text = txt;
    }


    public void StartOrStopTyping(){
        isTyping = !isTyping;
        chatCanvas.SetActive(isTyping);

        if (!isTyping)
            sendOffTextToOtherPerson();
    }

    public void getTextChatFromInput()
    {
        if (Input.GetKeyDown(KeyCode.A))
            chatText.text += "a";
    }


    public void sendOffTextToOtherPerson(){
        // Send message off

        chatText.text = "";
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


}
