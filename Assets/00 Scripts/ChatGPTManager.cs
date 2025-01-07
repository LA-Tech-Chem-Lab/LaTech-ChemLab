using UnityEngine;
using OpenAI;
using System.Collections.Generic;

public class ChatGPTManager : MonoBehaviour
{     
    public bool fastResponses;

    [TextArea(3, 10)] 
    public List<string> prompts;

    [TextArea(5, 15)]
    public string backup;

    private OpenAIApi openAI = new OpenAIApi();
    private List<ChatMessage> messages = new List<ChatMessage>();
    

    public async void SendMessageToChatGPT(string newText)
    {
        // Create a new message
        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = newText;
        newMessage.Role = "user";  // Set the role of the sender as "user"

        // Add the message to the list (even though you won't use the response)
        messages.Add(newMessage);

        // Create a request for the chat completion
        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = messages;
        if (fastResponses) request.Model = "gpt-3.5-turbo";  // Specify the model
        else request.Model = "gpt-4";
        // Send the request but do not process the response
        await openAI.CreateChatCompletion(request);

        // Optional: Log that the message was sent, but do not handle the response
        Debug.Log("Message sent: " + newText);
    }


    public async void AskChatGPT(string newText){
        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = newText;
        newMessage.Role = "user";

        messages.Add(newMessage);

        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = messages;
        if (fastResponses) request.Model = "gpt-3.5-turbo";  // Specify the model
        else request.Model = "gpt-4";

        var response = await openAI.CreateChatCompletion(request);

        if (response.Choices != null && response.Choices.Count > 0){
            var chatResponse = response.Choices[0].Message;
            messages.Add(chatResponse);

            Debug.Log(chatResponse.Content);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        string wholeMessage = "";
        
        foreach (string prompt in prompts)
            wholeMessage += prompt + "\n";

        SendMessageToChatGPT(wholeMessage);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
