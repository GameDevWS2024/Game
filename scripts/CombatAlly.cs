using System;
using System.Linq;

using Game.Scripts;

using Godot;
using Godot.Collections;

public partial class CombatAlly : Ally
{
    private float _attackCooldown = 0.5f; // Zeit zwischen Angriffen in Sekunden
    private float _timeSinceLastAttack = 0.0f; // Zeit seit dem letzten Angriff
    private const float AttackRange = 300.0f; // Distanz, innerhalb derer angegriffen wird
    private int _damage = 10; // Schaden pro Angriff
    [Export] PathFindingMovement _pathFindingMovement = null!;
    PackedScene _bulletScene = null!;


    public override void _Ready()
    {
        base._Ready(); // Ruft die Setup-Logik aus Ally auf
        AddToGroup("Entities");    
        _bulletScene = GD.Load<PackedScene>("res://Scenes/prefabs/BulletScene.tscn");
        GD.Print(_bulletScene.GetName()+"    .....");
    }

    public override void _PhysicsProcess(double delta)
    {
        _timeSinceLastAttack += (float)delta;
        //Check where ally is (darkness, bigger, smaller)
        SetAllyInDarkness();

        if (base._followPlayer)
        {
            _pathFindingMovement.TargetPosition = _player.GlobalPosition;
        }
        AttackNearestEnemy();
    }

    private void AttackNearestEnemy()
    {
        // Liste der Feinde abrufen
        Array<Node> enemyGroup = GetTree().GetNodesInGroup("Enemies");
        if (enemyGroup == null || enemyGroup.Count == 0)
        {
            return; // Keine Feinde zum Angreifen
        }

        // Nächsten Feind finden
        var nearestEnemy = enemyGroup
            .OfType<Node2D>()
            .Select(enemy => (enemy, distance: enemy.GlobalPosition.DistanceTo(GlobalPosition)))
            .OrderBy(t => t.distance)
            .FirstOrDefault();

        if (nearestEnemy.enemy != null)
        {
            Vector2 targetPosition = nearestEnemy.enemy.GlobalPosition;
            float distanceToTarget = targetPosition.DistanceTo(GlobalPosition);

            // Sich dem Ziel nähern
            _pathFindingMovement.TargetPosition = targetPosition;

            // Angreifen, wenn innerhalb der Reichweite und Abklingzeit abgelaufen
            if (distanceToTarget < AttackRange && _timeSinceLastAttack >= _attackCooldown)
            {
               // bulletNode = bulletScene.Instantiate();
               Node node = _bulletScene.Instantiate();
             //  GD.Print(node.GetType().Name); // Should print "Bullet"
             //  GD.Print(node is Bullet);     // Should print "True"
               if (node is not Bullet bullet)
               {
                   GD.Print("Instantiation or casting failed!");
               }
               else
               {
                   bullet.Initialize(GlobalPosition);
                   bullet.GlobalPosition = GlobalPosition;
                   bullet.SetTargetPosition(targetPosition);
                   GD.Print(bullet.GlobalPosition.ToString()+" pos.  "+bullet.GetTargetPosition().ToString());
               }
               
                
               if (nearestEnemy.enemy.HasNode("Health"))
               {
                    Health enemyHealth = nearestEnemy.enemy.GetNode<Health>("Health");
                    enemyHealth.Damage(_damage);
               }
               _timeSinceLastAttack = 0;
            }
        }
    }
}
