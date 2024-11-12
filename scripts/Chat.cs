using System;
using System.IO;
using System.Net.Mail;

using Game.Scripts.AI;

using Godot;
namespace Game.Scripts
{
    public partial class Chat : LineEdit
    {
        [Export(PropertyHint.File, "*.txt")]
        private string? _systemPromptFile;
        [Export]
        RichTextLabel? _richTextLabel;
        private string? _systemPrompt;
        private GeminiService? _geminiService;
        private readonly string _apiKeyPath = ProjectSettings.GlobalizePath("res://api_key.secret");
        private const string ChatPlaceholder = "Type here to chat";
        private const string EnterApiPlaceholder = "Enter API key";

        private void PrintResponse(string responsePice)
        {
            if (_richTextLabel != null)
            {
                _richTextLabel.Text = _richTextLabel.Text + responsePice;
            }
            GD.Print(responsePice + "\r");
        }

        public override void _Ready()
        {
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

        private void InitializeGeminiService()
        {

            try
            {
                _geminiService = new GeminiService(_apiKeyPath);

                if (_systemPrompt != null)
                {
                    _geminiService.SetSystemPrompt(_systemPrompt);
                }

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
            GD.Print($"Submitted text: {input}");

            if (_richTextLabel != null)
            {
                _richTextLabel.Text = "";
            }

            if (_geminiService != null)
            {
                await _geminiService.MakeQuerry(input, PrintResponse);
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
