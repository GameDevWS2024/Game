using Game.Scripts.Items;

using Godot;

namespace Game.Scripts;

public partial class Player : CharacterBody2D
{
    [Export] private float _maxSpeed = 400.0f; // Maximum speed
    [Export] private float _acceleration = 1200.0f; // How quickly we reach max speed
    [Export] private float _deceleration = 800.0f; // How quickly we slow down
    [Export] Sprite2D? _playerSprite;
    public Health Health = null!;
    private Core _core = null!;
    public AllyState CurrentState { get; private set; } = AllyState.SmallCircle;

    public enum AllyState
    {
        Darkness,
        SmallCircle,
        BigCircle
    }

    // Store the current velocity as a class field to maintain it between frames
    private Vector2 _currentVelocity = Vector2.Zero;
    private Player _player = null!;

    /*   // Player cant move if this is uncommented
	public Player(PlayerStats stats)
	{
		Stats = new PlayerStats(100, 100, 100, 100);
	}
	*/

    // Stats and Player initialization

    public override void _Ready()
    {
        AddToGroup("Entities");
        Health = GetNode<Health>("Health");
        _player = GetNode<Player>("%Player");
        _core = GetNode<Game.Scripts.Core>("%Core");
    }
    public PlayerStats Stats { get; private set; } = new PlayerStats(100, 10, 100, 50);

    public void Attack(Player target)
    {
        int damage = Stats.Strength;
        target.Stats.TakeDamage(damage);
        GD.Print($"{Name} attacked {target.Name} for {damage} damage.");
    }
    public void DisplayStats()
    {
        GD.Print($"Name: {Name}");
        GD.Print($"Health: {Stats.Health}");
        GD.Print($"Strength: {Stats.Strength}");
        GD.Print($"Speed: {Stats.Speed}");
        GD.Print($"Mana: {Stats.Mana}");
    }


    public override void _PhysicsProcess(double delta)
    {
        {
            Vector2 inputDirection = Vector2.Zero;
            float deltaFloat = (float)delta;

            // Get input direction
            if (Input.IsActionPressed("ui_up"))
            {
                if (CheckIfWouldBeInCircle(new Vector2(0, -1)))
                {
                    inputDirection.Y -= 1;    
                }
            }

            if (Input.IsActionPressed("ui_down"))
            {
                if (CheckIfWouldBeInCircle(new Vector2(0, 1)))
                {
                    inputDirection.Y += 1;    
                }
            }

            if (Input.IsActionPressed("ui_left"))
            {
                if (CheckIfWouldBeInCircle(new Vector2(-1, 0)))
                {
                    inputDirection.X -= 1;    
                }
                if (_playerSprite!.IsFlippedH())
                {
                    _playerSprite.SetFlipH(false);
                }
            }

            if (Input.IsActionPressed("ui_right"))
            {
                if (CheckIfWouldBeInCircle(new Vector2(1, 0)))
                {
                    inputDirection.X += 1;    
                }
                if (!_playerSprite!.IsFlippedH())
                {
                    _playerSprite.SetFlipH(true);
                }
            }

            // Normalize input direction to prevent faster diagonal movement
            inputDirection = inputDirection.Normalized();

            // Handle acceleration and deceleration
            if (inputDirection != Vector2.Zero)
            {
                // Accelerate when there's input
                _currentVelocity = _currentVelocity.MoveToward(
                    inputDirection * _maxSpeed,
                    _acceleration * deltaFloat
                );
            }
            else
            {
                // Decelerate when there's no input
                _currentVelocity = _currentVelocity.MoveToward(
                    Vector2.Zero,
                    _deceleration * deltaFloat
                );
            }

            // Update the velocity and move
            Velocity = _currentVelocity;
            MoveAndSlide();
            SetAllyInDarkness();
        }
    }

    public bool CheckIfWouldBeInCircle(Vector2 position)
    {
        return ((GlobalPosition + position).DistanceTo(_core.GlobalPosition) < _core.LightRadiusBiggerCircle || (GlobalPosition + position).DistanceTo(_core.GlobalPosition) < GlobalPosition.DistanceTo(_core.GlobalPosition));
    }

    public void SetAllyInDarkness()
        {
            // Berechne den Abstand zwischen Ally und Core
            Vector2 distance = this.Position - _core.Position;
            float distanceLength = distance.Length(); // Berechne die Länge des Vektors

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
}
