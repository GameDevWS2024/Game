using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Game.Scripts;
using Game.Scripts.Items;

using Godot;

using Vector2 = Godot.Vector2;

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
    [Export] public VisibleForAI[] AlwaysVisible = [];
    private bool _interactOnArrival = false;
    public bool FollowPlayer = true;
    private bool _busy;
    private bool _reached;
    private bool _harvest;
    private bool _returning;
    private Motivation _motivation = null!;
    private readonly static Inventory SInventory = new Inventory(36);
    private Player _player = null!;
    Chat? _chatNode;
    private GenerativeAI.Methods.ChatSession? _chat;
    private GeminiService? _geminiService;

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
        _chatNode = GetNode<Chat>("Chat");
        _geminiService = _chatNode.GeminiService;
        _chat = _geminiService!.Chat;
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

    private async void HandleTargetReached()
    {
        if (_interactOnArrival)
        {
            Interactable? interactable = GetCurrentlyInteractables().FirstOrDefault();
            interactable?.Trigger(this);
            _interactOnArrival = false;

            GD.Print("Interacted");
            List<VisibleForAI> visibleItems = GetCurrentlyVisible().Concat(AlwaysVisible).ToList();
            string visibleItemsFormatted = string.Join<VisibleForAI>("\n", visibleItems);
            string completeInput = $"Currently Visible:\n\n{visibleItemsFormatted}\n\n";

            string? arrivalResponse = await _geminiService!.MakeQuerry(completeInput + "\n Tell the commander about what new things you see now.");
            GD.Print("---: " + completeInput + "\n---: " + arrivalResponse + "---");
            RichTextLabel label = GetNode<RichTextLabel>("ResponseField");
            label.Text += "\n" + arrivalResponse;

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

    private void SetAllyInDarkness()
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

        _reached = GlobalPosition.DistanceTo(PathFindingMovement.TargetPosition) < 300;


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
                Vector2 targ = new Vector2(0, 500); // cl.GlobalPosition;
                // Target = core
                PathFindingMovement.TargetPosition = _core.GlobalPosition;
                GD.Print("Target position (should be CORE): " + PathFindingMovement.TargetPosition.ToString());
            }
            else
            {
                Location nearestLocation = Map.GetNearestItemLocation(new Location(GlobalPosition))!;
                //GD.Print("going to nearest loc("+nearestLocation.X +", "+nearestLocation.Y+") from "+ GlobalPosition.X + " " + GlobalPosition.Y);    //Target = nearest item
                PathFindingMovement.TargetPosition = nearestLocation.ToVector2();

            }
        }
    }

    private void HandleResponse(string response)
    {
        // extract relevant lines from output and differentiate between command and arguments
        if (response.Contains("follow"))
        {
            GD.Print("following");
            FollowPlayer = true;
            _busy = false;
            _returning = false;
        }

        if (response.Contains("STOP"))
        {
            _harvest = false;
            _busy = false;
            FollowPlayer = false;
        }

        if (response.Contains("GOTO"))
        {
            GD.Print("GOTO");
        }

        List<(string, string)> matches = ExtractRelevantLines(response);
        string richtext = "";
        foreach ((string op, string content) in matches)
        {
            string part = "";
            richtext += FormatPart(part, op, content);

            // differentiate what to do based on command op
            switch (op)
            {
                case "INTERACT":
                    _interactOnArrival = true;
                    break;
                // set motivation from output
                case "MOTIVATION":
                    _motivation.SetMotivation(content.ToInt());
                    break;
                // set follow to true
                case "FOLLOW":
                    GD.Print("following");
                    FollowPlayer = true;
                    _busy = false;
                    _returning = false;
                    break;
                // call goto with parsed coords
                case "GOTO" or "go to" or "Go To" or "GO TO":
                    {
                        const string goToPattern = @"^\s*\(\s*(-?\d+)\s*,\s*(-?\d+)\s*\)\s*$"; // Updated pattern
                        Match goToMatch = Regex.Match(content.Trim(), goToPattern);

                        if (goToMatch.Success)
                        {
                            FollowPlayer = false;
                            int x = int.Parse(goToMatch.Groups[1].Value);
                            int y = int.Parse(goToMatch.Groups[2].Value);
                            GD.Print(new Vector2(x, y).ToString());

                            GetNode<PathFindingMovement>("PathFindingMovement").GoTo(new Vector2(x, y));
                        }
                        else
                        {
                            GD.Print($"goto couldn't match the position, content was: '{content}'");
                        }
                        break;
                    }
                // if harvest command and not walking somewhere and items on map
                case "HARVEST" when !_busy && Map.Items.Count > 0:
                    GD.Print("harvesting");
                    Harvest();
                    break;
                // stop command stops ally from doing anything
                case "STOP":
                    _harvest = false;
                    _busy = false;
                    FollowPlayer = false;

                    break;
                // maybe for future
                case "REMEMBER":
                    break;
            }
        }
        _responseField.ParseBbcode(richtext); // formatted text into response field
    }

    private static string FormatPart(string part, string op, string content)
    {
        return part += op switch // format response based on different ops or response types
        {
            "THOUGHT" => "[i]" + content + "[/i]\n",
            "RESPONSE" or "COMMAND" or "STOP" => "[b]" + content + "[/b]\n",
            _ => content + "\n"
        };
    }

    private static List<(string, string)> ExtractRelevantLines(string response)
    {
        string[] lines = response.Split('\n').Where(line => line.Length > 0).ToArray();
        for (int i = 0; i < lines.Length; i++)
        {
            GD.Print(lines[i] + " - " + i);
        }
        List<(string, string)> matches = [];

        // Add commands to be extracted here
        List<String> ops = ["MOTIVATION", "THOUGHT", "RESPONSE", "REMEMBER", "GOTO", "HARVEST", "FOLLOW", "INTERACT", "STOP"];
        foreach (string line in lines)
        {
            foreach (string op in ops)
            {
                string pattern = op + @"[\s:]+.*";
                Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
                Match match = regex.Match(line);
                if (match.Success)
                {
                    matches.Add((op, match.Value.Substring(op.Length + 1).Trim())); // Extract the operand
                }
                else
                {
                    pattern = op + @":\s*(.*)"; // anstatt .* \d+ für zahlen
                    regex = new Regex(pattern);
                    match = regex.Match(line);

                    if (match is { Success: true, Groups.Count: > 1 })
                    {
                        matches.Add((op, match.Groups[1].Value));
                    }
                }
            }
        }

        response = "";
        return matches;
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
