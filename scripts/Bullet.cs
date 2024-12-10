using Godot;

namespace Game.Scripts
{
    public partial class Bullet : CharacterBody2D
    {
        [Export] public float Speed = 50f;
        private Vector2 _direction;

        public void Initialize(Vector2 position)
        {
            GlobalPosition = position;
        }

        public void SetTargetPosition(Vector2 targetPosition)
        {
            _direction = (targetPosition - GlobalPosition).Normalized();
            GD.Print(_direction.ToString() + " direction");
        }

        public Vector2 GetTargetPosition()
        {
            return _direction*Speed;
        }
        
        public override void _Process(double d)
        {
            Position += _direction * Speed * (float)d;
            GlobalPosition = Position;
            GD.Print(Position.ToString());
            
            // Remove bullet if it goes off-screen
            if (!GetViewportRect().HasPoint(GlobalPosition))
            {
                QueueFree();
            }
        }

        private void _on_Area2D_body_entered(Node body)
        {
            if (body.IsInGroup("Enemies")) // Replace with your group detection logic
            {
                // Handle damage or effects here
                QueueFree(); // Destroy the bullet on hit
            }
        }
    }
}
