using System;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Godot;
namespace Game.Scripts
{
    public partial class Chat : LineEdit
    {
        [Signal] public delegate void ResponseReceivedEventHandler(string response);

        [Export(PropertyHint.File, "*.txt")]
        private string? _systemPromptFile;

        private string _systemPrompt = "";
        private GeminiService? _geminiService;
        private readonly string _apiKeyPath = ProjectSettings.GlobalizePath("res://api_key.secret");
        private const string ChatPlaceholder = "Type here to chat";
        private const string EnterApiPlaceholder = "Enter API key";

        public override void _Ready()
        {
            TextSubmitted += OnTextSubmitted;

            string systemPromptAbsolutePath = ProjectSettings.GlobalizePath(_systemPromptFile);

            try
            {
                _systemPrompt = File.ReadAllText(systemPromptAbsolutePath);
            }
            catch (Exception ex)
            {
                GD.Print($"Failed to load systemPrompt. {ex.Message}");
            }

            //GD.Print(_systemPrompt);
            InitializeGeminiService();
        }

        public void SetSystemPrompt(string conversationHistory)
        {
            // Combine conversation history and the original system prompt
            string updatedPrompt = $"Instructions:\n{_systemPrompt}\n You remember: {conversationHistory}\n---------";

            _systemPrompt = updatedPrompt; // Update the current system prompt

            // Update the GeminiService instance
            _geminiService?.SetSystemPrompt(updatedPrompt);
        }


        private void InitializeGeminiService()
        {

            try
            {
                _geminiService = new GeminiService(_apiKeyPath);

                _geminiService.SetSystemPrompt(_systemPrompt);

                PlaceholderText = ChatPlaceholder;
            }

            catch (Exception ex)
            {
                GD.Print(ex.Message);
                PlaceholderText = EnterApiPlaceholder;
            }
        }

        public async Task<string?> SummarizeMessage(string conversation)
        {
            if (_geminiService != null)
            {
                try
                {
                    // You can modify the prompt here for the summarization request
                    string prompt = $"Extract only what needs to be remembered: {conversation}";
                    string? response = await _geminiService.MakeQuerry(prompt);
                    GD.Print("###"+response + " ist der verkürzte Conversation Context");

                    return response?.Trim();
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Failed to summarize conversation: {ex.Message}");
                    return null;
                }
            }

            GD.PrintErr("GeminiService not initialized.");
            return null;
        }
        
        public async Task<string?> SummarizeConversation(string conversation)
        {
            if (_geminiService != null)
            {
                try
                {
                    // You can modify the prompt here for the summarization request
                    string prompt = $"Extract only what needs to be remembered: {conversation}";
                    string? response = await _geminiService.MakeQuerry(prompt);
                    GD.Print("###"+response + " ist der verkürzte Conversation Context");

                    return response?.Trim();
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Failed to summarize conversation: {ex.Message}");
                    return null;
                }
            }

            GD.PrintErr("GeminiService not initialized.");
            return null;
        }
        
        private async void OnTextSubmitted(string input)
        {
            GD.Print($"Submitted text: {input}");

            if (_geminiService != null)
            {
                string? response = await _geminiService.MakeQuerry(input);
                if (response != null)
                {
                    EmitSignal(SignalName.ResponseReceived, response);
                }

                else
                {
                    GD.Print("No response");
                }

            }

            else
            {
                await File.WriteAllTextAsync(_apiKeyPath, input.Trim());
                InitializeGeminiService();
            }

            Clear();
        }
    }
}
