using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Game.Scripts;
using Game.Scripts.Items;
using System.Linq;
using System.Threading.Tasks;

using Godot;
using Vector2 = Godot.Vector2;
using System.Numerics;

public partial class Ally : CharacterBody2D
{
    private readonly List<string> _interactionHistory = [];
    [Export] private int _maxHistory = 5; // Number of interactions to keep
    public Health Health = null!;
    [Export] public Chat Chat = null!;
    [Export] RichTextLabel _responseField = null!;
    [Export] public PathFindingMovement PathFindingMovement = null!;
    [Export] private int _visionRadius = 300;
    [Export] private int _interactionRadius = 150;
    [Export] private Label _nameLabel = null!;
    [Export] public VisibleForAI[] AllwaysVisible = [];
    private bool _interactOnArrival = false;
    public bool FollowPlayer = true;
    private bool _busy;
    private bool _reached;
    private bool _harvest;
    private bool _returning;
    private Motivation _motivation = null!;
    private readonly static Inventory SInventory = new Inventory(36);
    private Player _player = null!;

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
        _motivation = GetNode<Motivation>("Motivation");
        Health = GetNode<Health>("Health");
        Chat.ResponseReceived += HandleResponse;
        _player = GetNode<Player>("%Player");

        _core = GetNode<Game.Scripts.Core>("%Core");
        Chat.Visible = false;
        PathFindingMovement.ReachedTarget += HandleTargetReached;
        Chat.ResponseReceived += HandleResponse;
    }

    private void HandleTargetReached()
    {
        if (_interactOnArrival)
        {
            Interactable? interactable = GetCurrentlyInteractables().FirstOrDefault();
            interactable?.Trigger(this);
            _interactOnArrival = false;

            GD.Print("Interacted");
        }
    }

    public List<VisibleForAI> GetCurrentlyVisible()
    {
        IEnumerable<VisibleForAI> visibleForAiNodes = GetTree().GetNodesInGroup(VisibleForAI.GroupName).OfType<VisibleForAI>();
        return visibleForAiNodes.Where(node => GlobalPosition.DistanceTo(node.GlobalPosition) <= _visionRadius).ToList();
    }
    public List<Interactable> GetCurrentlyInteractables()
    {
        IEnumerable<Interactable> interactable = GetTree().GetNodesInGroup(Interactable.GroupName).OfType<Interactable>();
        return interactable.Where(node => GlobalPosition.DistanceTo(node.GlobalPosition) <= _interactionRadius).ToList();
    }

    public void SetAllyInDarkness()
    {
        // Berechne den Abstand zwischen Ally und Core
        Vector2 distance = this.Position - _core.Position;
        float distanceLength = distance.Length();  // Berechne die Länge des Vektors

        // If ally further away than big circle, he is in the darkness
        if (distanceLength > Core.LightRadiusBiggerCircle)
        {
            CurrentState = AllyState.Darkness;
        }
        //if ally not in darkness and closer than the small Light Radius, he is in small circle
        else if (distanceLength < Core.LightRadiusSmallerCircle)
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

        if (GlobalPosition.DistanceTo(PathFindingMovement.TargetPosition) < 300)
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
        if (FollowPlayer && !_busy)
        {
            PathFindingMovement.TargetPosition = _player.GlobalPosition;
        }



        if (_harvest)
        {
            if (_returning)
            {
                PointLight2D cl = _core.GetNode<PointLight2D>("CoreLight");
                Vector2 targ = new Vector2(0, 500);  // cl.GlobalPosition;
                                                     // Target = core
                PathFindingMovement.TargetPosition = _core.GlobalPosition;
                                                           GD.Print("Target position (should be CORE): " + PathFindingMovement.TargetPosition.ToString());
            }
            else
            {
                Location nearestLocation = Map.GetNearestItemLocation(new Location(GlobalPosition))!;

                //GD.Print("going to nearest loc("+nearestLocation.X +", "+nearestLocation.Y+") from "+ GlobalPosition.X + " " + GlobalPosition.Y);
                //Target = nearest item
                PathFindingMovement.TargetPosition = nearestLocation.ToVector2();

            }
        }
    }

private async void HandleResponse(string response)
    {
        response = response.Replace("\"", ""); // Teile den String in ein Array anhand von '\n'

        string[] lines = response.Split('\n').Where(line => line.Length > 0).ToArray();
        List<(string, string)> matches = [];

        // Add commands to be extracted here
        List<String> ops = ["MOTIVATION", "THOUGHT", "RESPONSE", "REMEMBER", "GOTO"];
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
                    _motivation.SetMotivation(content.ToInt());
                }
                if (op == "GOTO"){
                    string goToPattern = @"\s(-?\d+)\s,\s*(-?\d+)";
                    Match goToMatch = Regex.Match(response, goToPattern);
                    if (goToMatch.Success)
                    {
                        GD.Print("Match successfully");
                        int x = int.Parse(goToMatch.Groups[1].Value);
                        int y = int.Parse(goToMatch.Groups[2].Value);
                        GD.Print(new Vector2(x, y).ToString());
                        GetNode<PathFindingMovement>("PathFindingMovement").GoTo(new Vector2(x, y));
                    }
                }
            }
        }

        _responseField.ParseBbcode(richtext);
        Chat.SetSystemPrompt(rememberText);

        // Update interaction history
        await UpdateInteractionHistoryAsync(rememberText, richtext);


        if (response.Contains("FOLLOW"))
        {
            GD.Print("following");
            FollowPlayer = true;
        }

        if (response.Contains("STOP"))
        {
            GD.Print("stop");
            FollowPlayer = false;
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
            //  GD.Print(""+summary+"");

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
        Chat.SetSystemPrompt(histAsString);
        _responseField.ParseBbcode(richtext + "\n" + rememberText);
    }

    private async Task<string> SummarizeConversationAsync(string conversation)
    {
        {
            string? summary = await Chat.SummarizeConversation(conversation);
            return summary ?? "Summary unavailable.";
        }
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
                Core.MaterialCount += item.Amount;
                Core.IncreaseScale();
                GD.Print("Increased scale");
            }
            SInventory.Clear();
            _busy = false; // Change busy state  
            _harvest = false; // Change harvest state
            _returning = false; // Change returning state
        }
    }
}
