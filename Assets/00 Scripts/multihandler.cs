using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using TMPro;
using UnityEngine.Rendering;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using System;



[System.Serializable] 
public class InputMessage
{
    public string message;
    public float timestamp;

    public InputMessage(string message, float timestamp)
    {
        this.message = message;
        this.timestamp = timestamp;
    }
}






public class multihandler : NetworkBehaviour
{   
    public GameObject currentPlayer;

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

    public TextMeshProUGUI chatText;
    public List<InputMessage> messageList = new List<InputMessage>();
    public TextMeshProUGUI messageListOnScreen;
    

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
        if (Input.GetKeyDown(KeyCode.Escape) && JoinCanvas.activeInHierarchy) // We are selecting server and press escape - Quit
            QuitGame();

        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab)) && !JoinCanvas.activeInHierarchy) // We are loaded in and press escape
            PauseOrUnpause();

        if (Input.GetKeyDown(KeyCode.Return) && !JoinCanvas.activeInHierarchy && !isPaused) // We press enter ONLY when we are in game
            StartOrStopTyping();
        // if (Input.GetKeyDown(KeyCode.Tab) && !isPaused)
        //     ToggleCursor();

        if (JoinCanvas.activeInHierarchy) { // Always show the mouse when selecting a server
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        if (isTyping)
            getTextChatFromInput();
        
        if (messageList.Count > 0)
            displayMessages();

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

        currentPlayer.GetComponent<playerMovement>().canMove = !isPaused;
        PauseCanvas.SetActive(isPaused);
        InGameCanvas.SetActive(!isPaused);

        if (isTyping) StartOrStopTyping();
    }


    public void setHelpText(string txt){
        helpText.text = txt;
    }


    public void StartOrStopTyping(){
        isTyping = !isTyping;
        currentPlayer.GetComponent<playerMovement>().isTyping = isTyping;
        currentPlayer.GetComponent<playerMovement>().updateTyping();
        chatCanvas.SetActive(isTyping);

        if (isTyping){          // We started typing, open the chat history and open mouse
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } else {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }


        if (!isTyping) // We pressed enter and ended our typing spree, send it off
            sendOffTextToOtherPerson();
    }

    public void getTextChatFromInput()
    {
        for (KeyCode key = KeyCode.A; key <= KeyCode.Z; key++)
        {
            if (Input.GetKeyDown(key))
            {
                bool isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                chatText.text += isShiftPressed ? key.ToString() : key.ToString().ToLower();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
            chatText.text += " ";
        
        if (Input.GetKeyDown(KeyCode.Backspace) && chatText.text.Length > 0)
            chatText.text = chatText.text.Substring(0, chatText.text.Length - 1);
    }


    public void sendOffTextToOtherPerson(){
        // Send message off
        if  (!string.IsNullOrEmpty(chatText.text)){
            // InputMessage currentMessage = new InputMessage(chatText.text, Time.time);
            // messageList.Add(currentMessage);
            SendChatMessageServerRpc(chatText.text);
        }
        
        chatText.text = "";
    }

    [ServerRpc(RequireOwnership = false)] // Allows any client to call this
    private void SendChatMessageServerRpc(string message, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"[SERVER] Received message: {message}");

        // Send message to all clients
        ReceiveChatMessageClientRpc(message);

        
    }

    
    [ClientRpc]
    private void ReceiveChatMessageClientRpc(string message)
    {
        Debug.Log($"[CLIENT] Received message: {message}");

        InputMessage currentMessage = new InputMessage(message, Time.time);
        messageList.Add(currentMessage);
    }

    public void displayMessages(){
        String allString = "";

        if (chatCanvas.activeInHierarchy){
            
            foreach (InputMessage message in messageList){
                allString += message.message + "\n";
            }   

        } else {
            foreach (InputMessage message in messageList){
                if (Time.time < message.timestamp + 5f){
                    allString += message.message + "\n";
                }
            }   
        }
        

        messageListOnScreen.text = allString;

        messageListOnScreen.transform.parent.gameObject.SetActive(allString.Length > 0 || chatCanvas.activeInHierarchy);
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
        
        // Find the local player instance
        currentPlayer = FindLocalPlayer();

    }
    private GameObject FindLocalPlayer()
    {
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            var networkObject = player.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsOwner)
            {
                return player;
            }
        }
        return null;
    }

}
