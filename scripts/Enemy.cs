using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

using Game.Scripts;

using Godot;
using Godot.Collections;

public partial class Enemy : CharacterBody2D
{
    [Export] PathFindingMovement _pathFindingMovement = null!;

    private bool _attack = true;
    private int _motivation;
    private Enemy? _enemy;
    private Array<Node>? _entityGroup;
    private CharacterBody2D? _player;
    private Node2D? _core;

    public override void _Ready()
    {
        _player = GetTree().CurrentScene.GetNode<CharacterBody2D>("%Player");
        _core = GetTree().CurrentScene.GetNode<Node2D>("%CORE");
    }

    private float _attackCooldown = 0.5f; // Time between attacks in seconds
    private float _timeSinceLastAttack = 0.0f; // Time accumulator
    private const float AttackRange = 110.0f; // Distance at which enemy can attack

    public override void _PhysicsProcess(double delta)
    {
        _timeSinceLastAttack += (float)delta;

        _entityGroup = GetTree().GetNodesInGroup("Entities");
        if (GetTree().GetNodesInGroup("Entities").ToList().Count == 0)
        {
            GetTree().CurrentScene.QueueFree();
            Node gameOverScene = GetNode("../scenes/prefabs/GameOver.tscn");
            GetTree().Root.AddChild(gameOverScene);
            // Doesnt quite work yet idk why
        }

        List<(Node2D entity, float distance)> nearestEntities = _entityGroup.OfType<Node2D>().Select(entity => (entity, entity.GlobalPosition.DistanceTo(GlobalPosition))).ToList();
        /*
        bool isNearbyCore = GlobalPosition.DistanceTo(_core!.GlobalPosition) < 100;
        nearestEntities = nearestEntities.Select(tup =>
            tup.entity.GetName() == "CORE" && isNearbyCore
                ? (tup.entity, tup.distance + 200)
                : tup
        ).ToList();
        */
        Node2D nearestEntity = nearestEntities.OrderBy(tup => tup.distance).FirstOrDefault().entity;

        if (nearestEntity != null)
        {
            Vector2 pos = nearestEntity.GlobalPosition;
            float distanceToTarget = pos.DistanceTo(this.GlobalPosition);
            if (_attack)
            {
                _pathFindingMovement.TargetPosition = pos;
                if (distanceToTarget < AttackRange && _timeSinceLastAttack >= _attackCooldown)
                {
                    // GD.Print(distanceToTarget+" < "+AttackRange+" --- "+_timeSinceLastAttack+" >= "+_attackCooldown+" s.");
                    if (nearestEntity.GetName() == "Player")
                    {
                        GD.Print("hit Player");
                        Health playerHealth = _player!.GetNode<Health>("Health");
                        playerHealth!.Call("Damage", 5);
                        GD.Print("new health: " + playerHealth.Amount);
                        _timeSinceLastAttack = 0;
                        if (playerHealth.Dead)
                        {
                            nearestEntity.QueueFree();
                        }
                    }

                    if (nearestEntity.GetName() == "Ally")
                    {
                        GD.Print("hit ally");
                        Health allieHealth = nearestEntity.GetNode<Health>("Health");
                        allieHealth!.Call("Damage", 5);
                        GD.Print("new ally health: " + allieHealth.Amount);
                        _timeSinceLastAttack = 0;
                        if (allieHealth.Dead)
                        {
                            nearestEntity.QueueFree();
                        }
                    }
                }
            }
        }
    }


}
