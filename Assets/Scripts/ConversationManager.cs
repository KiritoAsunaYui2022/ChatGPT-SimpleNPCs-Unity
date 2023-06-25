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


public class ConversationManager : MonoBehaviour
{
    public OpenAiTextChat openAiChat;

    [System.Serializable]
    public class Conversation
    {
        public string BotName;
        public List<ChatMessage> Messages;
    }

    public void SaveConversation(Conversation conversation)
    {
        string json = JsonUtility.ToJson(conversation);
        string path = Path.Combine(Application.persistentDataPath, $"{conversation.BotName}.json");
        File.WriteAllText(path, json);
        Debug.Log(path);
        //print()
    }

    public Conversation LoadConversation(string botName)
    {
        string path = Path.Combine(Application.persistentDataPath, $"{botName}.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<Conversation>(json);
        }
        else
        {
            return null; // or you can return a new Conversation
        }
    }
}
