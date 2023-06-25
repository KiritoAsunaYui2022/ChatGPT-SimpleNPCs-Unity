using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO; 

public class OpenAiTextChat : MonoBehaviour
{
    private GameObject chatUI;
    public GameObject conversationCamera;
    public AudioClip startSound;
    public AudioClip stopSound;
    public AudioClip sendSound;
    public AudioClip receiveSound;
    private AudioSource audioSource;
    private TMP_InputField inputField;
    private Button okButton;
    private TMP_Text outputField;
    private GameObject player;

    public BotIdle botIdle;

    private string filePath; //= @"C:\Users\...\BotConversations\Shopkeeper_conversations.txt"; //Hardcoded for debugging purposes. 

    [TextArea(5, 20)]   //minlines, maxlines
    public string initialRole = "You are a kind villager in the town of Vibeland";
    public string startString = "Approaching the villager..";
    public static string latestResponseMessage = "";

    private OpenAIAPI api;
    private List<ChatMessage> messages;


    void OnEnable()
    {
        botIdle = GetComponent<BotIdle>();

        //CHANGE THIS PATH TO YOUR OWN UNIQUE SETUP  
        filePath = $@"C:\Users\...\ChatGPT in Unity\BotConversations\{botIdle.botName}_conversations.txt"; 

        //Sound setup
        audioSource = this.GetComponent<AudioSource>();
        audioSource.clip = startSound;
        audioSource.Play();

        //Player setup
        player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<PlayerFunctions>().DisablePlayer();

        //Camera setup
        conversationCamera.SetActive(true);

        //UI Setup
        chatUI = GameObject.FindGameObjectWithTag("ChatUI");
        chatUI.GetComponent<Canvas>().enabled = true;
        inputField = chatUI.transform.GetComponentInChildren<TMP_InputField>();
        okButton = chatUI.transform.GetComponentInChildren<Button>();
        outputField = chatUI.transform.GetComponentInChildren<TMP_Text>();

        
        string apiKey = ""; //Hardcoded apiKey. Not recommended, but it's easier for a temporary use case. 
        api = new OpenAIAPI(apiKey); 
        //api = new OpenAIAPI(Environment.GetEnvironmentVariable("API_Key"), EnvironmentVariableTarget.User); //This would be a better use use case since your API Key is stored on your device rather than within your code. 
        okButton.onClick.AddListener(() => GetResponse());
        string botMemory = File.ReadAllText(filePath);


        messages = new List<ChatMessage>
        {

            new ChatMessage(ChatMessageRole.System, initialRole + botMemory)

        };

        //print("ROLE: " + initialRole); 

        inputField.text = "";
        outputField.text = "";

        Debug.Log(startString);
        outputField.text = startString;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) StopConversation();
        if (Input.GetKeyUp(KeyCode.Return)) okButton.onClick.Invoke();
    }
    void StopConversation()
    {
        audioSource.clip = stopSound;
        audioSource.Play();

        chatUI.GetComponent<Canvas>().enabled = false;
        conversationCamera.SetActive(false);

        player.GetComponent<PlayerFunctions>().EnablePlayer();

        gameObject.GetComponent<BotIdle>().enabled = true;
        this.enabled = false;

    }
 
    async void GetResponse()
    {
        if (this.enabled == false) return;
        if (inputField.text.Length < 1) return;

        audioSource.clip = sendSound;
        audioSource.Play();

        okButton.enabled = false;

        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;    //Sets role to user
         
        userMessage.Content = inputField.text;      //Sets content to what is in the text field
        //print("UserMessage: " + userMessage.Content); 

        File.AppendAllText(filePath, "Player: " + inputField.text + "\n");

        if (userMessage.Content.Length > 100) userMessage.Content = userMessage.Content.Substring(0, 100);  //Shortens the response 

        Debug.Log(string.Format("{0}: {1}", userMessage.rawRole, userMessage.Content)); //Logs user input
        outputField.text = string.Format("You: {0}", userMessage.Content);

        messages.Add(userMessage);

        inputField.text = "";

        try
        {
            var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo16k, //I added the GPT-3.5 Turbo 16k because reading the text in the text files can get pretty long, except within this demo. This is for future reference for other deeper projects.  
                Temperature = 0.1,
                MaxTokens = 90,
                Messages = messages
            });

            ChatMessage responseMessage = new ChatMessage();
            responseMessage.Role = chatResult.Choices[0].Message.Role;       //"Role" is sent to ChatGPT. 
            //Debug.Log("Response Role: " + responseMessage.Role); 
            responseMessage.Content = chatResult.Choices[0].Message.Content; //Get first response from what the latest response from the Assisstant is. 
            //Debug.Log("Message Content: " + responseMessage.Content); 
            latestResponseMessage = responseMessage.Content; //Same output as code above, but makes referencing easier.  

            File.AppendAllText(filePath, "You: " + latestResponseMessage + "\n");

            Debug.Log(string.Format("{0}: {1}", responseMessage.rawRole, responseMessage.Content)); //Logs response output
            //outputField.text = string.Format("You: {0}\n\n{1}: {2}", userMessage.Content, responseMessage.Content); //Throwing errors for some reason but you output could still be read in console. 
            outputField.text = string.Format("You: {0}\n\nBot: {1}", userMessage.Content, responseMessage.Content); //Thank you ChatGPT. I had no idea of how to fix this. 

            messages.Add(responseMessage);

            
        }
        catch (Exception e)
        {
            outputField.text = "Sorry, something went wrong: " + e; //Try and catch used to display errors in game
        }

        audioSource.clip = receiveSound;
        audioSource.Play();

        okButton.enabled = true;
    }
}
