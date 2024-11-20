using System;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;

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

        [Signal] public delegate void FollowPlayerInstructionEventHandler(bool follow, int speed);
        

        private string? _systemPrompt;
        private GeminiService? _geminiService;
        private readonly string _apiKeyPath = ProjectSettings.GlobalizePath("res://api_key.secret");
        private const string ChatPlaceholder = "Type here to chat";
        private const string EnterApiPlaceholder = "Enter API key";

        private void HandleResponse(string response)
        {
            if (_richTextLabel != null)
            {
                _richTextLabel.Text =  response;
            }
            GD.Print($"Response: {response}");

            string pattern = @"MOTIVATION:\s*(\d+)";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(response);  // Match against responsePice, not pattern

            int motivation = 5;

            if (match.Success && match.Groups.Count > 1)  // Check match.Success
            {
                try
                {
                    motivation = int.Parse(match.Groups[1].Value);
                }
                catch (Exception ex)
                {
                    GD.Print(ex);
                }
            }

            if (response.Contains("FOLLOW"))
            {
                    GD.Print("following");
                EmitSignal(SignalName.FollowPlayerInstruction, true, motivation * 25);
            }

            if (response.Contains("STOP"))
            {
                GD.Print("stop");
                EmitSignal(SignalName.FollowPlayerInstruction, false, motivation * 25);
            }

            GD.Print($"Motivation: {motivation}");
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

            if (_geminiService != null)
            {
                string? response = await _geminiService.MakeQuerry(input);
                if (response != null)
                {
                    HandleResponse(response);
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
