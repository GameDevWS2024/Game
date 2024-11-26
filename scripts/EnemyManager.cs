using System;
using System.Collections.Generic;
using System.Linq;

using Godot;

namespace Game.Scripts;

public partial class EnemyManager : Node2D
{
    [Export] public PackedScene? EnemyScene { get; set; }  // Set this in editor
    [Export] private float _minSpawnInterval = 10;
    [Export] private float _maxSpawnInterval = 60;
    [Export] private int _maxEnemies = 10;
    [Export] private float _baseInterval = 10f;
    [Export] private float _decayFactor = -3f; // -1.6f for regular game   // higher negative number means longer and longer duration between spawning

    private double _timeSinceLastSpawn = 0;


    private float CalculateSpawnInterval(int enemyCount)
    {
        float adjustedInterval = _baseInterval * Mathf.Pow(1 - (float)enemyCount / _maxEnemies, _decayFactor);
        return Mathf.Clamp(adjustedInterval, _minSpawnInterval, _maxSpawnInterval);
    }

    public override void _Process(double delta)
    {
        _timeSinceLastSpawn += (float)delta;

        List<Enemy> enemies = GetTree().GetNodesInGroup("Enemies").OfType<Enemy>().ToList();
        int enemyCount = enemies.Count;
        float spawnInterval = CalculateSpawnInterval(enemyCount);

        if (_timeSinceLastSpawn >= spawnInterval)
        {
            SpawnEnemy();
            _timeSinceLastSpawn = 0;
            GD.Print($"{enemyCount} enemies on the scene");
            GD.Print($"calculatedSpawnInterval: {spawnInterval}");
        }
    }

    private void SpawnEnemy()
    {
        if (EnemyScene == null)
        {
            return;
        }

        Enemy enemy = EnemyScene.Instantiate<Enemy>();
        AddChild(enemy);
        enemy.AddToGroup("Enemies"); // Add the enemy to the "Enemies" group

        // Set random position (adjust based on your needs)
        Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
        enemy.Position = new Vector2(
            Random.Shared.Next(0, (int)viewportSize.X),
            Random.Shared.Next(0, (int)viewportSize.Y)
        );
        GD.Print("spawned new enemy at " + enemy.GlobalPosition.X + ", " + enemy.GlobalPosition.Y);
    }
}
