using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Game.Scripts;
using System.Threading.Tasks;


using Game.Scripts.Items;

using Godot;

public partial class Ally : CharacterBody2D
{
    private readonly List<string> _interactionHistory = [];
    [Export] private int _maxHistory = 5; // Number of interactions to keep

    public Health Health = null!;
    public Motivation Motivation = null!;

    [Export] Chat _chat = null!;
    [Export] RichTextLabel _responseField = null!;
    [Export] PathFindingMovement _pathFindingMovement = null!;
    [Export] private Label _nameLabel = null!;
    public bool _followPlayer = true;
    private bool _busy;
    private bool _reached;
    private bool _harvest;
    private bool _returning;
    private int _motivation;
    public Player _player = null!;
    private readonly static Inventory SInventory = new Inventory(36);

    //Enum with states for ally in darkness, in bigger or smaller circle for map damage system
    public enum AllyState
    {
        Darkness,
        SmallCircle,
        BigCircle
    }

    public AllyState CurrentState { get; private set; } = AllyState.SmallCircle;

    private Game.Scripts.Core _core = null!;

    public override void _Ready()
    {
        base._Ready();
        Motivation = GetNode<Motivation>("Motivation");
        Health = GetNode<Health>("Health");
        _player = GetNode<Player>("%Player");

        _core = GetNode<Game.Scripts.Core>("%Core");
        _chat.ResponseReceived += HandleResponse;
    }

    public void SetAllyInDarkness()
    {
        // Berechne den Abstand zwischen Ally und Core
        Vector2 distance = this.Position - _core.Position;
        float distanceLength = distance.Length();  // Berechne die Länge des Vektors

        // If ally further away than big circle, he is in the darkness
        if (distanceLength > _core.LightRadiusBiggerCircle)
        {
            CurrentState = AllyState.Darkness;
        }
        //if ally not in darkness and closer than the small Light Radius, he is in small circle
        else if (distanceLength < _core.LightRadiusSmallerCircle)
        {
            CurrentState = AllyState.SmallCircle;
        }
        //if ally not in darkness and not in small circle, ally is in big circle
        else
        {
            CurrentState = AllyState.BigCircle;
        }

    }

    public override void _PhysicsProcess(double delta)
    {
        //Check where ally is (darkness, bigger, smaller)
        SetAllyInDarkness();

        UpdateTarget();

        if (GlobalPosition.DistanceTo(_pathFindingMovement.TargetPosition) < 300)
        {
            _reached = true;
        }
        else
        {
            _reached = false;
        }


        if (_harvest && _reached) // Harvest logic
        {
            Harvest();
        }
    }

    private void UpdateTarget()
    {
        if (_followPlayer && !_busy)
        {
            _pathFindingMovement.TargetPosition = _player.GlobalPosition;
        }



        if (_harvest)
        {
            if (_returning)
            {
                PointLight2D cl = _core.GetNode<PointLight2D>("CoreLight");
                Vector2 targ = new Vector2(0, 500);  // cl.GlobalPosition;
                // Target = core
                _pathFindingMovement.TargetPosition = targ; //_core.GlobalPosition;
                //GD.Print("Target position (should be CORE): " + _pathFindingMovement.TargetPosition.ToString());
            }
            else
            {
                Location nearestLocation = Map.GetNearestItemLocation(new Location(GlobalPosition))!;

                //GD.Print("going to nearest loc("+nearestLocation.X +", "+nearestLocation.Y+") from "+ GlobalPosition.X + " " + GlobalPosition.Y);
                //Target = nearest item
                _pathFindingMovement.TargetPosition = nearestLocation.ToVector2();

            }
        }
    }

    private async void HandleResponse(string response)
    {
        response = response.Replace("\"", ""); // Teile den String in ein Array anhand von '\n'

        string[] lines = response.Split('\n').Where(line => line.Length > 0).ToArray();
        List<(string, string)> matches = [];

        // Add commands to be extracted here
        List<String> ops = ["MOTIVATION", "THOUGHT", "RESPONSE", "REMEMBER"];
        foreach (string line in lines)
        {
            foreach (string op in ops)
            {
                string pattern = op + @":\s*(.*)"; // anstatt .* \d+ für zahlen
                Regex regex = new Regex(pattern);
                Match match = regex.Match(line);
                if (match is { Success: true, Groups.Count: > 1 })
                {
                    matches.Add((op, match.Groups[1].Value));
                }

            }
        }

        string richtext = "", rememberText = "";
        foreach ((string op, string content) in matches)
        {
            if (op == "REMEMBER")
            {
                rememberText = content;
            }
            else
            {
                richtext += op switch
                {
                    "THOUGHT" => "[i]" + content + "[/i]\n",
                    "RESPONSE" or "COMMAND" => "[b]" + content + "[/b]\n",
                    _ => content + "\n"
                };
                if (op == "MOTIVATION")
                {
                    GD.Print("set motivation to:" + content.ToInt());
                    Motivation.SetMotivation(content.ToInt());
                }
            }
        }

        _responseField.ParseBbcode(richtext);
        _chat.SetSystemPrompt(rememberText);

        // Update interaction history
        await UpdateInteractionHistoryAsync(rememberText, richtext);


        if (response.Contains("FOLLOW"))
        {
            GD.Print("following");
            _followPlayer = true;
        }

        if (response.Contains("STOP"))
        {
            GD.Print("stop");
            _followPlayer = false;
        }

        if (response.Contains("HARVEST") && !_busy)
        {
            GD.Print("harvesting");
            if (Map.Items.Count > 0)
            {
                _harvest = true; // Change harvest state
                _busy = true; // Change busy state
            }
        }
    }
    private async Task UpdateInteractionHistoryAsync(string rememberText, string richtext)
    {
        GD.Print(_interactionHistory.Count + " memory units full");
        string histAsString = "";
        foreach (string hist in _interactionHistory)
        {
            histAsString += hist;
        }
        // Check if history exceeds the maximum size
        if (_interactionHistory.Count > _maxHistory)
        {
            GD.Print("summarizing:");
            // Summarize the whole conversation history
            string summary = await SummarizeConversationAsync(histAsString);
            //  GD.Print("***"+summary+"***");

            // Replace history with the summary
            _interactionHistory.Clear();
            _interactionHistory.Add(summary);
        }
        // string currentSummary = await SummarizeConversationAsync(newInteraction); 
        _interactionHistory.Add(rememberText); //currentSummary
        histAsString = "";
        foreach (string hist in _interactionHistory)
        {
            histAsString += hist;
            GD.Print(hist + "#");
        }
        _chat.SetSystemPrompt(histAsString);
        _responseField.ParseBbcode(richtext + "\n" + rememberText);
    }

    private async Task<string> SummarizeConversationAsync(string conversation)
    {
        {
            string? summary = await _chat.SummarizeConversation(conversation);
            return summary ?? "Summary unavailable.";
        }
    }

    public string GetConversationHistory()
    {
        return string.Join("\n", _interactionHistory);
    }
        private void Harvest()
    {
        if (!_returning)
        {
            // extract the nearest item and add to inventory (pickup)
            if (SInventory.HasSpace()) // if inventory has space
            {
                GD.Print("harvesting...");
                Itemstack item = Map.ExtractNearestItemAtLocation(new Location(GlobalPosition));
                GD.Print(item.Material + " amount: " + item.Amount);
                SInventory.AddItem(item); // add item to inventory
                SInventory.Print();
            } // if inventory has no space don't harvest it
            else
            {
                GD.Print("No space");
            }

            _returning = true;
        }
        else
        {
            // Empty inventory into the core

            foreach (Itemstack item in SInventory.GetItems())
            {
                if (item.Material == Game.Scripts.Items.Material.None)
                {
                    continue;
                }
                _core.MaterialCount += item.Amount;
                _core.IncreaseScale();
                GD.Print("Increased scale");
            }
            SInventory.Clear();
            _busy = false; // Change busy state  
            _harvest = false; // Change harvest state
            _returning = false; // Change returning state
        }
    }
}
