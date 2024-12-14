using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Game.Scripts;

using Godot;

public partial class Ally : CharacterBody2D
{
    public Health Health = null!;
    [Export] Chat _chat = null!;
    [Export] RichTextLabel _responseField = null!;
    [Export] PathFindingMovement _pathFindingMovement = null!;
    [Export] private Label _nameLabel = null!;
    [Export] private int _visionRadius = 100;
    [Export] private int _interactionRadius = 100;
    private bool _followPlayer = false;
    private int _motivation;
    private Player _player = null!;
    private bool _interactOnArrival = false;

    [Export] public VisibleForAI[] AllwaysVisible = [];

    //Enum with states for ally in darkness, in bigger or smaller circle for map damage system
    public enum AllyState
    {
        Darkness,
        SmallCircle,
        BigCircle
    }

    public AllyState CurrentState { get; private set; } = AllyState.SmallCircle;

    private Core _core = null!;

    public override void _Ready()
    {
        Health = GetNode<Health>("Health");
        _player = GetNode<Player>("%Player");
        _core = GetNode<Core>("%Core");

        _pathFindingMovement.TargetPosition = GlobalPosition;

        _pathFindingMovement.ReachedTarget += HandleTargetReached;
        _chat.ResponseReceived += HandleResponse;
    }

    public override void _PhysicsProcess(double delta)
    {
        SetAllyInDarkness();
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

    private void HandleResponse(string response)
    {
        _responseField.Text = response;

        string pattern = @"MOTIVATION:\s*(\d+)";
        Regex regex = new Regex(pattern);
        Match match = regex.Match(response);

        if (match is { Success: true, Groups.Count: > 1 })
        {
            try
            {
                _motivation = int.Parse(match.Groups[1].Value);
            }
            catch (Exception ex)
            {
                GD.Print(ex);
            }
        }

        string goToPattern = @"GO TO:\s*(-?\d+)\s*,\s*(-?\d+)";

        Match goToMatch = Regex.Match(response, goToPattern);

        if (goToMatch.Success)
        {
            int x = int.Parse(goToMatch.Groups[1].Value);
            int y = int.Parse(goToMatch.Groups[2].Value);

            _pathFindingMovement.TargetPosition = new Vector2(x, y);
        }

        if (response.Contains("INTERACT"))
        {
            _interactOnArrival = true;
        }

        GD.Print($"Motivation: {_motivation}");
    }
}
