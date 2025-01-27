using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Game.Scripts.AI;

using Godot;
namespace Game.Scripts
{
    public partial class Chat : LineEdit
    {
        [Signal] public delegate void ResponseReceivedEventHandler(string response);

        [Export(PropertyHint.File, "ally_system_prompt.txt")]
        private string? _systemPromptFile;

        //  [Export(PropertyHint.File, "introduction_ally_system_prompt.txt")]
        //  private string? _introductionSystemPromptFile;

        private Scripts.Ally _ally = null!;

        public string SystemPrompt = "";
        //    private string _introductionSystemPrompt = "";
        public GeminiService? GeminiService;
        private readonly string _apiKeyPath = ProjectSettings.GlobalizePath("res://api_key.secret");
        private const string ChatPlaceholder = "Type here to chat";
        private const string EnterApiPlaceholder = "Enter API key";
        private int _responseCount;
        public List<VisibleForAI> AlreadySeen = [];
        private Godot.Collections.Array<Node> _entityList = null!;
        private VisibleForAI _ally1VisibleForAi = null!;
        private VisibleForAI _ally2VisibleForAi = null!;

        public override void _Ready()
        {
            _ally = GetParent().GetParent<Ally>();
            _responseCount = 0;
            TextSubmitted += OnTextSubmitted;
            AlreadySeen = _ally.AlwaysVisible.ToList();

            string systemPromptAbsolutePath = ProjectSettings.GlobalizePath(_systemPromptFile);
            //   string introductionSystemPromptAbsolutePath = ProjectSettings.GlobalizePath(_introductionSystemPromptFile);

            SystemPrompt = File.ReadAllText(systemPromptAbsolutePath); // Load system prompt into SystemPrompt
                                                                       // _introductionSystemPrompt = File.ReadAllText(introductionSystemPromptAbsolutePath); // Load intro prompt

            InitializeGeminiService(SystemPrompt); // Pass system prompt to InitializeGeminiService
            foreach (Ally ally in GetTree().GetNodesInGroup("Entities").OfType<Ally>())
            {
                if (ally.GetName().ToString().Contains('2'))
                {
                    _ally2VisibleForAi = ally.GetNode<VisibleForAI>("VisibleForAI");
                    AlreadySeen.Add(_ally2VisibleForAi);
                }
                else
                {
                    _ally1VisibleForAi = ally.GetNode<VisibleForAI>("VisibleForAI");
                    AlreadySeen.Add(_ally1VisibleForAi);
                }
            }
        }

        public async void SeenItems()
        {
            List<VisibleForAI> newItems = [];
            List<VisibleForAI> visibleItems = _ally.GetCurrentlyVisible();

            if (visibleItems.Count > 0)
            {
                foreach (VisibleForAI item in visibleItems)
                {
                    bool isContains = false;
                    foreach (VisibleForAI vfai in AlreadySeen)
                    {
                        if (vfai != null) 
                        {
                            if (vfai == item) {isContains=true; break;}
                        }
                    }
                    if (!isContains && item.NameForAi.Trim() != "")
                    {
                        AlreadySeen.Add(item);
                        newItems.Add(item);
                    }
                }
            }
            if (newItems.Count > 0)
            {
                GD.Print("prompt");
                string alreadySeenFormatted = string.Join<VisibleForAI>("\n", AlreadySeen);
                string newItemsFormatted = string.Join<VisibleForAI>("\n", newItems);
                string completeInput = $"New Objects:\n\n{newItemsFormatted}\n\n" + $"Already Seen:\n\n{alreadySeenFormatted}\n\n" + "Player: ";

                GD.Print($"-------------------------\nInput:\n{completeInput}");

                string? response = await GeminiService!.MakeQuery(completeInput);
                if (response != null)
                {
                    EmitSignal(SignalName.ResponseReceived, response);
                    GD.Print($"----------------\nResponse:\n{response}");
                }
                else
                {
                    GD.Print("No response");
                }

                newItems.Clear();
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
            SeenItems();
        }
        private void InitializeGeminiService(string systemPrompt)
        {
            try
            {
                GeminiService = new GeminiService(_apiKeyPath, systemPrompt); // Pass system prompt to GeminiService constructor
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
            string alreadySeenFormatted = string.Join<VisibleForAI>("\n", AlreadySeen);
            string completeInput = $"New Objects:\n\n\n\n" + $"Already Seen:\n\n{alreadySeenFormatted}\n\n" + $"Player: {input}";
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
                InitializeGeminiService(SystemPrompt);
            }

            Clear();
        }
    }
}
