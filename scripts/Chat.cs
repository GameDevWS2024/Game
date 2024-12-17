using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;

using Godot;
namespace Game.Scripts
{
    public partial class Chat : LineEdit
    {
        [Signal] public delegate void ResponseReceivedEventHandler(string response);

        [Export(PropertyHint.File, "*.txt")]
        private string? _systemPromptFile;

        private Ally _ally = null!;

        private string _systemPrompt = "";
        private GeminiService? _geminiService;
        private readonly string _apiKeyPath = ProjectSettings.GlobalizePath("res://api_key.secret");
        private const string ChatPlaceholder = "Type here to chat";
        private const string EnterApiPlaceholder = "Enter API key";

        public override void _Ready()
        {
            TextSubmitted += OnTextSubmitted;

            _ally = GetParent<Ally>();

            string systemPromptAbsolutePath = ProjectSettings.GlobalizePath(_systemPromptFile);

            try
            {
                _systemPrompt = File.ReadAllText(systemPromptAbsolutePath);
            }
            catch (Exception ex)
            {
                GD.Print($"Failed to load systemPrompt. {ex.Message}");
            }

            GD.Print(_systemPrompt);
            InitializeGeminiService();
        }

        public void SetSystemPrompt(string newPrompt)
        {
            _systemPrompt = newPrompt;  // Ändere den System-Prompt

            // Stelle sicher, dass die GeminiService-Instanz mit dem neuen Prompt aktualisiert wird
            if (_geminiService != null)
            {
                _geminiService.SetSystemPrompt(_systemPrompt);
            }
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

        private async void OnTextSubmitted(string input)
        {

            List<VisibleForAI> visibleItems = _ally.GetCurrentlyVisible().Concat(_ally.AllwaysVisible).ToList();
            string visibleItemsFormatted = string.Join<VisibleForAI>("\n", visibleItems);

            string completeInput = $"Currently Visible:\n\n{visibleItemsFormatted}\n\nPlayer: {input}";

            GD.Print($"-------------------------\nInput:\n{completeInput}");

            if (_geminiService != null)
            {
                string? response = await _geminiService.MakeQuerry(completeInput);
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
