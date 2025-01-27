using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        private List<VisibleForAI> _alreadySeen = [];
        private Godot.Collections.Array<Node> _entityList = null!;
        private VisibleForAI _ally1VisibleForAi = null!;
        private VisibleForAI _ally2VisibleForAi = null!;

        public override void _Ready()
        {
            _ally = GetParent().GetParent<Ally>();
            _responseCount = 0;
            TextSubmitted += OnTextSubmitted;
            _alreadySeen = _ally.AlwaysVisible.ToList();

            string systemPromptAbsolutePath = ProjectSettings.GlobalizePath(_systemPromptFile);
            string introductionSystemPromptAbsolutePath = ProjectSettings.GlobalizePath(_introductionSystemPromptFile);

            _entityList = GetTree().GetNodesInGroup("Entities");
            foreach (Ally ally in _entityList.OfType<Ally>())
            {
                if (ally.GetName().ToString().Contains('2'))
                {
                    _ally2VisibleForAi = ally.GetNode<VisibleForAI>("VisibleForAI");
                }
                else
                {
                    _ally1VisibleForAi = ally.GetNode<VisibleForAI>("VisibleForAI");
                }
            }

            _alreadySeen.Add(_ally1VisibleForAi);
            _alreadySeen.Add(_ally2VisibleForAi);

            //GD.Print(_systemPrompt);
            InitializeGeminiService();
        }

        public async void SeenItems(){
            List<VisibleForAI> newItems = [];
            List<VisibleForAI> visibleItems = _ally.GetCurrentlyVisible();
            string visibleItemsFormatted = string.Join<VisibleForAI>("\n", visibleItems);

            if(visibleItems.Count > 0){
                foreach(VisibleForAI item in visibleItems){
                    if(!_alreadySeen.Contains(item) && item != null){
                        _alreadySeen.Add(item);
                        newItems.Add(item);
                    }
                }
            }
            if(newItems.Count > 0){
            GD.Print("prompt");
            string alreadySeenFormatted = string.Join<VisibleForAI>("\n", _alreadySeen);
            string newItemsFormatted = string.Join<VisibleForAI>("\n", newItems);
            string completeInput = $"New Objects:\n\n{newItemsFormatted}\n\n"+ $"Already Seen:\n\n{alreadySeenFormatted}\n\n" + $"Player: {""}";
            
            GD.Print($"-------------------------\nInput:\n{completeInput}");

            string originalSystemPrompt = SystemPrompt;
            SystemPrompt = "In the following you'll get a list of things you see with coordinates. Respond by telling the commander just what might be important or ask clarifying questions on what to do next. \n";
            string? arrivalResponse = await GeminiService!.MakeQuery(completeInput);
            List<(string, string)>? responseGroups = Ally.ExtractRelevantLines(arrivalResponse!);
            foreach ((string, string) response in responseGroups)
            {
                if (response.Item1 == "RESPONSE")
                {
                    GD.Print(response.Item2);
                }
            }
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
            newItems.Clear();
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
            SeenItems();
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
            string alreadySeenFormatted = string.Join<VisibleForAI>("\n", _alreadySeen);
            string completeInput = $"New Objects:\n\n\n\n"+ $"Already Seen:\n\n{alreadySeenFormatted}\n\n" + $"Player: {input}";
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
