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
    [Export] PathFindingMovement _pathFindingMovement;

    private bool _attack = true;
    private int _motivation;
    private Enemy _enemy;
    private Array<Node> entityGroup;
    private int? dist;
    private CharacterBody2D _player;
    private Node2D _core;

    public override void _Ready()
    {
        _enemy = GetNode<Enemy>("%Enemy");
        _player = GetNode<CharacterBody2D>("%Player");
        _core = GetNode<Node2D>("%CORE");
    }

    public override void _PhysicsProcess(double delta)
    {
        
        //foreach (Node2D entity in entityGroup)
       // {
          //  int currDist = (int) entity.GlobalPosition.DistanceTo(_player.GlobalPosition);
          //   if (dist < currDist)
           //  {
          //       dist = currDist; 
        //        pos = entity.GlobalPosition;
      //      }
    //    }
        entityGroup = GetTree().GetNodesInGroup("Entities");
        List<(Node2D entity, float distance)> nearestEntities = entityGroup.OfType<Node2D>().Select(entity => (entity, entity.GlobalPosition.DistanceTo(GlobalPosition))).ToList();
       // /*
        foreach ((Node2D entity, float distance) tup in nearestEntities)
        {
            (Node2D entity, float distance) valueTuple = tup;
            if (valueTuple.entity.GetName() == "Player")
            {
                valueTuple.distance -= 100;
            }
            GD.Print(tup);
        }
       // */

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
