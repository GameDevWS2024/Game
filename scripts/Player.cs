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
	public readonly Items.Inventory Inventory = new Items.Inventory(36);

	/*   // Player cant move if this is uncommented
	public Player(PlayerStats stats)
	{
		Stats = new PlayerStats(100, 100, 100, 100);
	}
	*/

	// Stats and Player initialization

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
	}
}
