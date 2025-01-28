using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using GenerativeAI.Exceptions;
using GenerativeAI.Methods;
using GenerativeAI.Models;
using GenerativeAI.Types;

using Godot;

namespace Game.Scripts.AI;

public class GeminiService
{
    public bool Busy = false;
    private readonly GenerativeModel _model;
    public ChatSession Chat;
    readonly Queue<string> promptQueue = new Queue<string>();




    public GeminiService(string apiKeyFilePath, string systemPrompt) // Add systemPrompt parameter
    {
        if (string.IsNullOrEmpty(apiKeyFilePath))
        {
            throw new ArgumentNullException(nameof(apiKeyFilePath), "API key file path cannot be null or empty");
        }
        if (!File.Exists(apiKeyFilePath))
        {
            throw new FileNotFoundException($"API key file not found at path: {apiKeyFilePath}");
        }
        try
        {
            // Read the API key from the file, trimming any whitespace
            string apiKey = File.ReadAllText(apiKeyFilePath).Trim();
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("API key file is empty");
            }
            _model = new GenerativeModel(apiKey);

            _model.SafetySettings =
            [
                new SafetySetting { Category = HarmCategory.HARM_CATEGORY_HARASSMENT, Threshold = HarmBlockThreshold.BLOCK_NONE },
                new SafetySetting { Category = HarmCategory.HARM_CATEGORY_HATE_SPEECH, Threshold = HarmBlockThreshold.BLOCK_NONE },
                new SafetySetting { Category = HarmCategory.HARM_CATEGORY_SEXUALLY_EXPLICIT, Threshold = HarmBlockThreshold.BLOCK_NONE },
                new SafetySetting { Category = HarmCategory.HARM_CATEGORY_DANGEROUS_CONTENT, Threshold = HarmBlockThreshold.BLOCK_NONE }
            ];

            Chat = _model.StartChat(new StartChatParams());

            // Send system prompt as the first message in the chat history
            if (!string.IsNullOrEmpty(systemPrompt))
            {
                Task.Run(async () => await Chat.SendMessageAsync(systemPrompt)).Wait(); // Run synchronously for initialization. Be cautious in real async scenarios.
            }
        }
        catch (Exception ex) when (ex is not FileNotFoundException && ex is not ArgumentNullException)
        {
            throw new Exception($"Error reading API key from file: {ex.Message}", ex);
        }
    }

    // Remove SetSystemPrompt method entirely as it's likely no longer needed

    public async Task<string?> MakeQuery(string input)
    {
        string? result = null;
        int ctr = 0;
        // Enqueue zum reinmachen
        // dequeue zum rausmachen
        // trypeek zum reinschauen ohne entfernen
        // clear() zum löschen
        promptQueue.Enqueue(input);
        while (Busy) { }
        Busy = true;
        GD.Print(input);
        // while (ctr <= 3 && result == null) // try for 3 times in case an error occurs
        //  {
        //       ctr++;
        //     if (ctr > 1)
        //     {
        //          GD.Print("tried "+ctr+" times.");
        //     }

        try
        {
            result = await Chat.SendMessageAsync(input);
        }
        catch (Exception ex)
        {
            //Task.Delay(2000).Wait();  // wait for 2 seconds function here
            throw new Exception($"Error getting Gemini response: {ex.Message}", ex);
        }
        //    }
        Busy = false;

        if (result == null)
        {
            throw new GenerativeAIException("doesn't respond", "on: " + input);
        }
        return result;
    }

    public IReadOnlyList<Content> GetChatHistory()
    {
        return Chat.History;
    }

    public void ClearChat()
    {
        Chat = _model.StartChat(new StartChatParams());
    }
}

