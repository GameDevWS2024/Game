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

    private readonly Queue<string> _queryQueue = new Queue<string>();
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public async Task<string?> MakeQuery(string input)
    {
        _queryQueue.Enqueue(input);
        return await ProcessQueryQueue();
    }

    private async Task<string?> ProcessQueryQueue()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_queryQueue.Count == 0)
            {
                return null;
            }

            string input = _queryQueue.Dequeue();

            string? result = null;
            try
            {
                result = await Chat.SendMessageAsync(input);
            }
            catch (Exception ex)
            {
                GD.Print("failed");
                await Task.Delay(2000);
                throw new Exception($"Network error getting Gemini response: {ex.Message}", ex);
            }

            GD.Print("got response of length: " + result.Length + ". Waiting for: " + (int)(1000 * 0.015f * result.Length) + " ms.");
            await Task.Delay((int)(1000 * 0.015f * result.Length));

            return result;
        }
        finally
        {
            _semaphore.Release();
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

