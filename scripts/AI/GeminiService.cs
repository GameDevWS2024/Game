using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;

using GenerativeAI.Methods;
using GenerativeAI.Models;
using GenerativeAI.Types;

namespace Game.Scripts.AI
{
    public class GeminiService
    {
        private readonly GenerativeModel _model;
        private ChatSession _chat;

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
                _chat = _model.StartChat(new StartChatParams());
            }
            catch (Exception ex) when (ex is not FileNotFoundException && ex is not ArgumentNullException)
            {
                throw new Exception($"Error reading API key from file: {ex.Message}", ex);
            }
        }

        public async void SetSystemPrompt(string prompt)
        {
            await _chat.SendMessageAsync(prompt);
        }

        public async Task? MakeQuerry(string input, Action<string> outputHandler)
        {
            try
            {
                await _chat.StreamContentAsync(input, outputHandler);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting Gemini response: {ex.Message}", ex);
            }
        }

        public IReadOnlyList<Content> GetChatHistory()
        {
            return _chat.History;
        }

        public void ClearChat()
        {
            _chat = _model.StartChat(new StartChatParams());
        }
    }
}
