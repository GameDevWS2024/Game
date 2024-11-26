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
        _enemy = GetNode<Enemy>("%Enemy");
        _player = GetNode<CharacterBody2D>("%Player");
        _core = GetNode<Node2D>("%CORE");
    }

    public override void _PhysicsProcess(double delta)
    {
        // bool isNearbyCore = this.GlobalPosition.DistanceTo(_core!.GlobalPosition) < 250;
        // GD.Print("enemy is nearby core, attacking player preferably");
        bool isNearbyCore = true;
        _entityGroup = GetTree().GetNodesInGroup("Entities");
        List<(Node2D entity, float distance)> nearestEntities = _entityGroup.OfType<Node2D>().Select(entity => (entity, entity.GlobalPosition.DistanceTo(GlobalPosition))).ToList();
        nearestEntities = nearestEntities.Select(tup =>
            tup.entity.GetName() == "Player" && isNearbyCore
                ? (tup.entity, tup.distance - 150)
                : tup
        ).ToList();

        Node2D nearestEntity = nearestEntities.OrderBy(tup => tup.distance).FirstOrDefault().entity;

        if (nearestEntity != null)
        {
            Vector2 pos = nearestEntity.GlobalPosition;

            if (_attack)
            {
                _pathFindingMovement.TargetPosition = pos;
            }
        }
    }


}
