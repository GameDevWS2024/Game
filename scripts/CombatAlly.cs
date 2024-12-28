using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Game.Scripts;

using Godot;
using Godot.Collections;

public partial class CombatAlly : CharacterBody2D
{
    public Health Health = null!;
    [Export] public Chat _chat = null!;
    [Export] RichTextLabel _responseField = null!;
    [Export] public PathFindingMovement _pathFindingMovement = null!;
    [Export] private Label _nameLabel = null!;
    public bool _followPlayer = true;
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

    private Game.Scripts.Core _core = null!;

    private float _attackCooldown = 0.5f; // Time between attacks in seconds
    private float _timeSinceLastAttack = 0.0f; // Time accumulator
    private const float AttackRange = 170.0f; // Distance at which ally can attack
    private int _damage = 10; // Damage dealt to enemies

    public override void _Ready()
    {
        Health = GetNode<Health>("Health");
        _chat.ResponseReceived += HandleResponse;
        _player = GetNode<Player>("%Player");
        _core = GetNode<Game.Scripts.Core>("%Core");
        _chat.Visible = false;
    }

    public void SetAllyInDarkness()
    {
        // Calculate the distance between Ally and Core
        Vector2 distance = this.Position - _core.Position;
        float distanceLength = distance.Length(); // Get the length of the vector

        // If ally further away than big circle, he is in the darkness
        if (distanceLength > Core.LightRadiusBiggerCircle)
        {
            CurrentState = AllyState.Darkness;
        }
        // If ally not in darkness and closer than the small Light Radius, he is in small circle
        else if (distanceLength < Core.LightRadiusSmallerCircle)
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

    private void HandleResponse(string response)
    {
        _responseField.Text = response;

        GD.Print($"Response: {response}");

        string pattern = @"MOTIVATION:\s*(\d+)";
        Regex regex = new Regex(pattern);
        Match match = regex.Match(response);

        if (match.Success && match.Groups.Count > 1)
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

        if (response.Contains("FOLLOW"))
        {
            GD.Print("Following");
            _followPlayer = true;
            _pathFindingMovement.gotoCommand = false;
        }

        if (response.Contains("STOP"))
        {
            GD.Print("Stop");
            _followPlayer = false;
        }

        if (response.Contains("DEFEND"))
        {
            GD.Print("Following and defending Player");
            _followPlayer = true;
        }

        GD.Print($"Motivation: {_motivation}");
    }
}
