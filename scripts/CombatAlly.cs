using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Game.Scripts;

using Godot;
using Godot.Collections;

public partial class CombatAlly : Game.scripts.Ally
{
    public new Health Health = null!;
    [Export] public new Chat Chat = null!;
    [Export] RichTextLabel _responseField = null!;
    [Export] public new PathFindingMovement PathFindingMovement = null!;
    [Export] private Label _nameLabel = null!;
    private int _motivation;
    
    private float _attackCooldown = 0.5f; // Time between attacks in seconds
    private float _timeSinceLastAttack = 0.0f; // Time accumulator
    private const float AttackRange = 170.0f; // Distance at which ally can attack
    private int _damage = 10; // Damage dealt to enemies

    public override void _Ready()
    {
        Health = GetNode<Health>("Health");
        _core = GetNode<Core>("%Core");
    }
    

    public override void _PhysicsProcess(double delta)
    {
        _timeSinceLastAttack += (float)delta;

        // Check where ally is (darkness, bigger, smaller)
        SetAllyInDarkness();
        AttackNearestEnemy();
    }

    private void AttackNearestEnemy()
    {
        Array<Node> enemyGroup = GetTree().GetNodesInGroup("Enemies");
        if (enemyGroup == null || enemyGroup.Count == 0)
        {
            return;
        }

        // Find the nearest enemy
        List<(Node2D enemy, float distance)> nearestEnemies = enemyGroup
            .OfType<Node2D>()
            .Select(enemy => (enemy, distance: enemy.GlobalPosition.DistanceTo(GlobalPosition)))
            .ToList();
        Node2D? nearestEnemy = nearestEnemies.OrderBy(t => t.distance).FirstOrDefault().enemy;

        if (nearestEnemy == null)
        {
            return;
        }

        Vector2 targetPosition = nearestEnemy.GlobalPosition;
        float distanceToTarget = targetPosition.DistanceTo(GlobalPosition);

        // Move toward the target
        PathFindingMovement.TargetPosition = targetPosition;

        if (!(distanceToTarget < AttackRange) || !(_timeSinceLastAttack >= _attackCooldown))
        {
            return;
        }

        if (nearestEnemy.HasNode("Health"))
        {
            Health enemyHealth = nearestEnemy.GetNode<Health>("Health");
            enemyHealth.Damage(_damage);
        }
        _timeSinceLastAttack = 0;
    }
}
