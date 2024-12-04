using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Game.Scripts;

using Godot;
using Godot.Collections;

public partial class CombatAlly : CharacterBody2D
{
    private readonly List<string> _interactionHistory = [];
    [Export] private int _maxHistory = 5; // Number of interactions to keep
    public Health Health = null!;
    public Motivation Motivation = null!;
    [Export] Chat _chat = null!;
    [Export] RichTextLabel _responseField = null!;
    [Export] PathFindingMovement _pathFindingMovement = null!;
    [Export] private Label _nameLabel = null!;
    private bool _followPlayer = true;
    private int _motivation;
    private Player _player = null!;

    // Enum with states for ally in darkness, in bigger or smaller circle for map damage system
    public enum AllyState
    {
        Darkness,
        SmallCircle,
        BigCircle
    }

    public AllyState CurrentState { get; private set; } = AllyState.SmallCircle;

    private Core _core = null!;
    private float _attackCooldown = 0.5f; // Time between attacks in seconds
    private float _timeSinceLastAttack = 0.0f; // Time accumulator
    private const float AttackRange = 170.0f; // Distance at which ally can attack
    private int _damage = 10; // Damage dealt to enemies
    public override void _Ready()
    {
        AddToGroup("Entities");
        base._Ready();
        Health = GetNode<Health>("Health");
        Motivation = GetNode<Motivation>("Motivation");
        _chat.ResponseReceived += HandleResponse;
        _player = GetNode<Player>("%Player");
        _core = GetNode<Core>("%Core");
        _chat.ResponseReceived += HandleResponse;
    }

    public void SetAllyInDarkness()
    {
        // Calculate the distance between Ally and Core
        Vector2 distance = this.Position - _core.Position;
        float distanceLength = distance.Length(); // Get the length of the vector

        // If ally further away than big circle, he is in the darkness
        if (distanceLength > _core.LightRadiusBiggerCircle)
        {
            CurrentState = AllyState.Darkness;
        }
        // If ally not in darkness and closer than the small Light Radius, he is in small circle
        else if (distanceLength < _core.LightRadiusSmallerCircle)
        {
            CurrentState = AllyState.SmallCircle;
        }
        // If ally not in darkness and not in small circle, ally is in big circle
        else
        {
            CurrentState = AllyState.BigCircle;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        _timeSinceLastAttack += (float)delta;

        // Check where ally is (darkness, bigger, smaller)
        SetAllyInDarkness();

        if (_followPlayer)
        {
            _pathFindingMovement.TargetPosition = _player.GlobalPosition;
        }

        AttackNearestEnemy();
    }

    private void AttackNearestEnemy()
    {
        // Get the list of enemies
        Array<Node> enemyGroup = GetTree().GetNodesInGroup("Enemies");
        if (enemyGroup == null || enemyGroup.Count == 0)
        {
            return; // No enemies to attack
        }

        // Find the nearest enemy
        List<(Node2D enemy, float distance)> nearestEnemies = enemyGroup
            .OfType<Node2D>()
            .Select(enemy => (enemy, distance: enemy.GlobalPosition.DistanceTo(GlobalPosition)))
            .ToList();

        Node2D? nearestEnemy = nearestEnemies.OrderBy(t => t.distance).FirstOrDefault().enemy;

        if (nearestEnemy != null)
        {
            Vector2 targetPosition = nearestEnemy.GlobalPosition;
            float distanceToTarget = targetPosition.DistanceTo(GlobalPosition);

            // Move toward the target
            _pathFindingMovement.TargetPosition = targetPosition;

            // Attack if within range and cooldown allows
            if (distanceToTarget < AttackRange && _timeSinceLastAttack >= _attackCooldown)
            {
                if (nearestEnemy.HasNode("Health"))
                {
                    Health enemyHealth = nearestEnemy.GetNode<Health>("Health");
                    enemyHealth.Damage(_damage);
                }
                _timeSinceLastAttack = 0;
            }
        }
    }

    private async void HandleResponse(string response)
    {
        response = response.Replace("\"", ""); // Teile den String in ein Array anhand von '\n'
                                               // GD.Print(response);

        string[] lines = response.Split('\n').Where(line => line.Length > 0).ToArray();
        List<(string, string)> matches = [];
        List<String> ops = ["MOTIVATION", "THOUGHT", "RESPONSE", "REMEMBER"];
        // Gib die Elemente des Arrays mit foreach aus
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

        string richtext = "";
        string conversationContext = "";
        string rememberText = "";
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

                conversationContext += content;
            }
        }

        _responseField.ParseBbcode(richtext);

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

        //  GD.Print($"Motivation: {_motivation}");
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
}
