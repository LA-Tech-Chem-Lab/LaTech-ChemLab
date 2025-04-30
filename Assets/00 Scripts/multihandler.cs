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
using UnityEngine.SceneManagement;



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






public class multihandler : MonoBehaviour
{   
    public GameObject currentPlayer;
    public Vector3 startingPosition;
    public bool hasLeftSpawn;
    public bool hasWornGoggles;
    public GameObject giveWalterSolutionText;
    public progessTracker progressTrackerScript;
    public GameObject putOnGogglesText;
    private interactWithObjects interactWithObjectsScript;

    //public GameObject JoinCanvas;
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
    public GameObject helpTextBG;
    public float helpAreaWhiteness;
    public GameObject gogglesHaze;

    [Header("Text Chat")]
    public bool isTyping; // Necessary 
    // public GameObject chatCanvas;

    // public TextMeshProUGUI chatText;
    // public List<InputMessage> messageList = new List<InputMessage>();
    // public TextMeshProUGUI messageListOnScreen;
    
    [Header("Teacher Chat")]
    public ChatGPTManager teacherAI;
    public GameObject teacher;
    public GameObject TeacherAskInputFieldObject;
    public TMP_InputField inputField;
    public TextMeshProUGUI inputFieldText;
    public GameObject TeacherResponseCanvas;
    public TextMeshProUGUI teacherResponseTextBox;
    float timeOfLastResponse; float timeOfLastQuestion;
    public float timeSinceLastQuestion;
    public float timeSinceLastResponse;
    

    private void Start()
    {
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        InGameCanvas.SetActive(true);

        timeOfLastResponse = Time.time - 11f; timeOfLastQuestion = timeOfLastResponse;
        interactWithObjectsScript = currentPlayer.GetComponent<interactWithObjects>();

        startingPosition = currentPlayer.transform.position;
        progressTrackerScript = GetComponent<progessTracker>();
        teacher = teacherAI.transform.parent.gameObject;
    }

    void Update()
    {   
        // if (Input.GetKeyDown(KeyCode.Escape)) // We are selecting server and press escape - Quit
        //     QuitGame();

        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))) // We are loaded in and press escape - pause or unpause
            PauseOrUnpause();

        if (Input.GetKeyDown(KeyCode.Return)&& !isPaused) // We press enter ONLY when we are in game
            StartOrStopTyping();

        if (Input.GetKeyDown(KeyCode.T) && !isTyping){
            if (!isPaused) PauseOrUnpause();
            TeacherAskInputFieldObject.SetActive(true);
            inputField.Select();
            inputField.ActivateInputField();
        }

        gogglesHaze.SetActive(interactWithObjectsScript.gogglesOn);
        helpTextBG.SetActive(helpText.text.Length > 0);

        // if (Input.GetKeyDown(KeyCode.Tab) && !isPaused)
        //     ToggleCursor();

        // if (JoinCanvas.activeInHierarchy) { // Always show the mouse when selecting a server
        //     Cursor.lockState = CursorLockMode.None;
        //     Cursor.visible = true;
        // }
        
        textChatAndAIChat();

        if (Vector3.Distance(currentPlayer.transform.position, startingPosition) > 5f)
            hasLeftSpawn = true;
        
        
        if (!hasWornGoggles && interactWithObjectsScript.gogglesOn)
            hasWornGoggles = true;

        if (hasLeftSpawn && !interactWithObjectsScript.gogglesOn && !hasWornGoggles)
            putOnGogglesText.SetActive(true);
        
        if (!hasLeftSpawn || interactWithObjectsScript.gogglesOn)
            putOnGogglesText.SetActive(false);
        
        print(progressTrackerScript.currentState);
        // print(Vector3.Distance(currentPlayer.transform.position, teacher.transform.position));
    }

    void textChatAndAIChat(){
        // if (isTyping)
        //     getTextChatFromInput();
        
        // if (messageList.Count > 0)
        //     displayMessages();

        timeSinceLastResponse = Time.time - timeOfLastResponse;
        timeSinceLastQuestion = Time.time - timeOfLastQuestion;

        TeacherResponseCanvas.SetActive(timeSinceLastResponse <= 15f);
    }

    public void QuitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #endif
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    } 

    public void ToggleCursor()
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
        ToggleCursor();

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
        // chatCanvas.SetActive(isTyping);

        if (isTyping){          // We started typing, open the chat history and open mouse
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } else {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }


        
    }

    // public void getTextChatFromInput()
    // {
    //     for (KeyCode key = KeyCode.A; key <= KeyCode.Z; key++)
    //     {
    //         if (Input.GetKeyDown(key))
    //         {
    //             bool isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    //             chatText.text += isShiftPressed ? key.ToString() : key.ToString().ToLower();
    //         }
    //     }
    //     // Handle digits
    //     for (KeyCode key = KeyCode.Alpha0; key <= KeyCode.Alpha9; key++)
    //     {
    //         if (Input.GetKeyDown(key))
    //         {
    //             bool isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    //             string keyStr = key.ToString().Replace("Alpha", ""); // Convert KeyCode.AlphaX to "X"
                
    //             if (isShiftPressed)
    //             {
    //                 string[] shiftSymbols = { ")", "!", "@", "#", "$", "%", "^", "&", "*", "(" };
    //                 chatText.text += shiftSymbols[int.Parse(keyStr)];
    //             }
    //             else
    //                 chatText.text += keyStr;
    //         }
    //     }

    //     if (Input.GetKeyDown(KeyCode.Period))  chatText.text += ".";
    //     if (Input.GetKeyDown(KeyCode.Space))  chatText.text += " ";
    //     if (Input.GetKeyDown(KeyCode.Comma))  chatText.text += ",";
        
    //     if (Input.GetKeyDown(KeyCode.Backspace) && chatText.text.Length > 0)
    //         chatText.text = chatText.text.Substring(0, chatText.text.Length - 1);
    // }


    // public void sendOffTextToOtherPerson(){
    //     // Send message off
    //     if  (!string.IsNullOrEmpty(chatText.text)){
    //         // InputMessage currentMessage = new InputMessage(chatText.text, Time.time);
    //         // messageList.Add(currentMessage);
    //         SendChatMessageServerRpc(chatText.text);
    //     }
        
    //     chatText.text = "";
    // }

    // [ServerRpc(RequireOwnership = false)] // Allows any client to call this
    // private void SendChatMessageServerRpc(string message, ServerRpcParams rpcParams = default)
    // {
    //     Debug.Log($"[SERVER] Received message: {message}");

    //     // Send message to all clients
    //     // ReceiveChatMessageClientRpc(message);

        
    // }

    
    // [ClientRpc]
    // private void ReceiveChatMessageClientRpc(string message)
    // {
    //     Debug.Log($"[CLIENT] Received message: {message}");

    //     InputMessage currentMessage = new InputMessage(message, Time.time);
    //     messageList.Add(currentMessage);
    // }

    // public void displayMessages(){
    //     String allString = "";

    //     if (chatCanvas.activeInHierarchy){
            
    //         foreach (InputMessage message in messageList){
    //             allString += message.message + "\n";
    //         }   

    //     } else {
    //         foreach (InputMessage message in messageList){
    //             if (Time.time < message.timestamp + 5f){
    //                 allString += message.message + "\n";
    //             }
    //         }   
    //     }
        

    //     messageListOnScreen.text = allString;

    //     messageListOnScreen.transform.parent.gameObject.SetActive(allString.Length > 0 || chatCanvas.activeInHierarchy);
    // }
    
    public void updateQuestionTime(){
        timeOfLastQuestion = Time.time;
    }

    public void sendQuestionToTeacher(String textInput){
        teacherAI.AskChatGPT(textInput);
        inputField.text = ""; // Clear the input field
        inputField.ActivateInputField(); // Keep focus
    }

    public void ReceiveResponseFromTeacher(String response){
        teacherResponseTextBox.text = response;
        timeOfLastResponse = Time.time;
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
        // JoinCanvas.SetActive(false);
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
