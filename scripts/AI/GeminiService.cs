using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;

using GenerativeAI.Methods;
using GenerativeAI.Models;
using GenerativeAI.Types;

using Godot;

public class GeminiService
{
    private readonly GenerativeModel _model;
    public ChatSession Chat;

    public GeminiService(string apiKeyFilePath)
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
        }
        catch (Exception ex) when (ex is not FileNotFoundException && ex is not ArgumentNullException)
        {
            throw new Exception($"Error reading API key from file: {ex.Message}", ex);
        }
    }

    public async void SetSystemPrompt(string prompt)
    {
        try
        {
            // GD.Print(prompt);
            await Chat.SendMessageAsync(prompt);
        }
        catch (Exception e)
        {
            throw new Exception("Error setting system prompt", e);
        }
    }
    public async Task<string?> MakeQuery(string input)
    {
        GD.Print(input);
        try
        {
            return await Chat.SendMessageAsync(input);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting Gemini response: {ex.Message}", ex);
        }
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
