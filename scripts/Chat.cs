using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Godot;
namespace Game.Scripts
{
    public partial class Chat : LineEdit
    {
        [Signal] public delegate void ResponseReceivedEventHandler(string response);

        [Export(PropertyHint.File, "ally_system_prompt.txt")]
        private string? _systemPromptFile;

        [Export(PropertyHint.File, "introduction_ally_system_prompt.txt")]
        private string? _introductionSystemPromptFile;

        private Scripts.Ally _ally = null!;

        public string SystemPrompt = "";
        private string _introductionSystemPrompt = "";
        public GeminiService? GeminiService;
        private readonly string _apiKeyPath = ProjectSettings.GlobalizePath("res://api_key.secret");
        private const string ChatPlaceholder = "Type here to chat";
        private const string EnterApiPlaceholder = "Enter API key";
        private int _responseCount;

        public override void _Ready()
        {
            _ally = GetParent().GetParent<Ally>(); ;
            _responseCount = 0;
            TextSubmitted += OnTextSubmitted;

            string systemPromptAbsolutePath = ProjectSettings.GlobalizePath(_systemPromptFile);
            string introductionSystemPromptAbsolutePath = ProjectSettings.GlobalizePath(_introductionSystemPromptFile);

            try
            {
                SystemPrompt = File.ReadAllText(systemPromptAbsolutePath);
                _introductionSystemPrompt = File.ReadAllText(introductionSystemPromptAbsolutePath);
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
            string updatedPrompt = "";
            if (_responseCount <= 3)
            {
                GD.Print("responding initially");
                updatedPrompt = $"Instructions:\n{_introductionSystemPrompt}\n You remember: {conversationHistory}\n---------";
            }
            else
            {
                GD.Print("responding normally");
                // Combine conversation history and the original system prompt
                updatedPrompt = $"Instructions:\n{SystemPrompt}\n You remember: {conversationHistory}\n---------";
            }
            //  _systemPrompt = updatedPrompt; // Update the current system prompt
            // Update the GeminiService instance
            GeminiService?.SetSystemPrompt(updatedPrompt);
        }


        private void InitializeGeminiService()
        {
            try
            {
                GeminiService = new GeminiService(_apiKeyPath);

                GeminiService.SetSystemPrompt(SystemPrompt);

                PlaceholderText = ChatPlaceholder;
            }

            catch (Exception ex)
            {
                GD.Print(ex.Message);
                PlaceholderText = EnterApiPlaceholder;
            }
        }


        private async void OnTextSubmitted(string input)
        {

            List<VisibleForAI> visibleItems = _ally.GetCurrentlyVisible().Concat(_ally.AlwaysVisible).ToList();
            string visibleItemsFormatted = string.Join<VisibleForAI>("\n", visibleItems);

            string completeInput = $"Currently Visible:\n\n{visibleItemsFormatted}\n\nPlayer: {input}";
            GD.Print($"-------------------------\nInput:\n{completeInput}");

            if (GeminiService != null)
            {
                string? response = await GeminiService.MakeQuery(completeInput);
                if (response != null)
                {
                    EmitSignal(SignalName.ResponseReceived, response);
                    GD.Print($"----------------\nResponse:\n{response}");
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
