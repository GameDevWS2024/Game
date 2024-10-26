using Godot;

public partial class Player : CharacterBody2D
{
    [Export]
    private float _maxSpeed = 400.0f;      // Maximum speed
    [Export]
    private float _acceleration = 1200.0f;  // How quickly we reach max speed
    [Export]
    private float _deceleration = 800.0f;   // How quickly we slow down

    // Store the current velocity as a class field to maintain it between frames
    private Vector2 _currentVelocity = Vector2.Zero;

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
        }

        if (Input.IsActionPressed("ui_right"))
        {
            inputDirection.X += 1;
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
    }
}
