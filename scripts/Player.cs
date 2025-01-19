using Game.Scripts.Items;

using Godot;

namespace Game.Scripts;

public partial class Player : CharacterBody2D
{
    [Export] private float _maxSpeed = 400.0f; // Maximum speed
    [Export] private float _acceleration = 1200.0f; // How quickly we reach max speed
    [Export] private float _deceleration = 800.0f; // How quickly we slow down
    [Export] Sprite2D? _playerSprite;

    // Store the current velocity as a class field to maintain it between frames
    private Vector2 _currentVelocity = Vector2.Zero;
    public Core Core = null!;
    private Player _player = null!;
    public Health Health = null!;
    public AllyState CurrentState { get; private set; } = AllyState.SmallCircle;
    public enum AllyState
    {
        Darkness,
        SmallCircle,
        BigCircle
    }

    public override void _Ready()
    {
        Health = GetNode<Health>("Health");
        _player = GetNode<Player>("%Player");
        Core = GetNode<Core>("%Core");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 inputDirection = Vector2.Zero;
        float deltaFloat = (float)delta;

        // Get input direction
        if (Input.IsActionPressed("ui_up"))
        {
            inputDirection.Y -= 1;
        }

        if (Input.IsActionPressed("ui_down"))
        {
            inputDirection.Y += 1;
        }

        if (Input.IsActionPressed("ui_left"))
        {
            inputDirection.X -= 1;
            if (_playerSprite!.IsFlippedH())
            {
                _playerSprite.SetFlipH(false);
            }
        }

        if (Input.IsActionPressed("ui_right"))
        {
            inputDirection.X += 1;
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
    public void SetAllyInDarkness()
    {
        // Berechne den Abstand zwischen Ally und Core
        Vector2 distance = this.Position - Core.Position;
        float distanceLength = distance.Length();  // Berechne die Länge des Vektors

        // If ally further away than big circle, he is in the darkness
        if (distanceLength > Core.LightRadiusBiggerCircle)
        {
            CurrentState = AllyState.Darkness;
        }
        //if ally not in darkness and closer than the small Light Radius, he is in small circle
        else if (distanceLength < Core.LightRadiusSmallerCircle)
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
