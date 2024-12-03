using System.Collections.Generic;
using System.Linq;

using Godot;
using Godot.Collections;

public partial class Enemy : CharacterBody2D
{
    [Export] PathFindingMovement _pathFindingMovement = null!;
    [Export] private int _damage = 5;
    [Signal] public delegate void DeathEventHandler();
    [Signal] public delegate void HealthChangedEventHandler(int newHealth);

    
    private bool _attack = true;
    private Array<Node>? _entityGroup;
    private CharacterBody2D? _player;
    private Node2D? _core;
    public Health Health = null!;

    public override void _Ready()
    {
        Health = GetNode<Health>("Health");
        _player = GetTree().CurrentScene.GetNode<CharacterBody2D>("%Player");
        _core = GetTree().CurrentScene.GetNode<Node2D>("%Core");
    }

    private float _attackCooldown = 0.5f; // Time between attacks in seconds
    private float _timeSinceLastAttack = 0.0f; // Time accumulator
    private const float AttackRange = 110.0f; // Distance at which enemy can attack

    public override void _PhysicsProcess(double delta)
    {
        // temporary cause I couldnt figure out how to hide/delete Enemy like Player or Allie
        if (Health.Amount <= 0)
        {
            QueueFree();
        }
        
        //
        
        _timeSinceLastAttack += (float)delta;

        _entityGroup = GetTree().GetNodesInGroup("Entities");
        if (GetTree().GetNodesInGroup("Entities").ToList().Count == 0)
        {
            GetTree().CurrentScene.QueueFree();
            Node gameOverScene = GD.Load<PackedScene>("res://scenes/prefabs/GameOver.tscn").Instantiate();
            GetTree().Root.AddChild(gameOverScene);
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
                    Health allieHealth = nearestEntity.GetNode<Health>("Health");
                    allieHealth.Damage(_damage);
                    _timeSinceLastAttack = 0;
                }
            }
        }
    }


}
