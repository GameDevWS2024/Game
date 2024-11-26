using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Godot;

namespace Game.scripts;

public partial class EnemyManager : Node2D
{
    [Export] public PackedScene? EnemyScene { get; set; }  // Set this in editor

    private Thread? _spawnThread;
    private volatile bool _isSpawning = false;

    private int _minSpawnTime = 15;  // 15 second
    private int _maxSpawnTime = 60;  // 30 seconds
    // change these values to adjust enemy spawning rate curve
    private float _minSpawnInterval = 10f;
    private int _maxEnemies = 10;
    private float _baseInterval = 10f;
    private float _decayFactor = -3f; // -1.6f for regular game   // higher negative number means longer and longer duration between spawning

    private int CalculateSpawnInterval(int enemyCount)
    {
        float adjustedInterval = _baseInterval * Mathf.Pow(1 - (float)enemyCount / _maxEnemies, _decayFactor);
        return 1000 * (int)Mathf.Min(Mathf.Max(adjustedInterval, _minSpawnInterval), _maxSpawnTime);
    }
    public override void _Ready()
    {
        StartSpawning();
    }

    private void StartSpawning()
    {
        _isSpawning = true;
        _spawnThread = new Thread(() =>
        {
            Random random = new Random();

            while (_isSpawning)
            {
                try
                {
                    List<Enemy> enemies = GetTree().GetNodesInGroup("Enemies").OfType<Enemy>().ToList();
                    int enemyCount = enemies.Count;
                    GD.Print(enemyCount + " enemies on the scene");

                    //int delay = random.Next(_minSpawnTime, _maxSpawnTime);
                    // calculate delay with formula such that if many enemies are alive, it slows down
                    int delay = CalculateSpawnInterval(enemyCount);
                    GD.Print("calculatedSpawnInterval: " + delay / 1000);

                    Thread.Sleep(delay);
                    CallDeferred("SpawnEnemy"); // Wenn am Ende von PhysicsProcess Funktion und kurze Leerlaufzeit ist
                }
                catch (ThreadInterruptedException)
                {
                    break;
                }
            }
        });

        _spawnThread.Start();
    }

    public void StopSpawning()
    {
        _isSpawning = false;
        _spawnThread?.Interrupt();
        _spawnThread?.Join();
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

    public void SetSpawnInterval(int minMs, int maxMs)
    {
        _minSpawnTime = minMs;
        _maxSpawnTime = maxMs;
    }

    public override void _ExitTree()
    {
        StopSpawning();
        base._ExitTree();
    }
}
