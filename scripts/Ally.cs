using System;
using Godot;

namespace Game.scripts
{
    public partial class Ally : CharacterBody2D
    {
        [Export] private NodePath _playerPath;

        private CharacterBody2D _player;
        private NavigationAgent2D _navigationAgent;
        private const float Speed = 150.0f;
        private Vector2 _velocity = Vector2.Zero;
        private const double accel = 2;
        private Random ran = new Random();
        private int ctr = 0;
        private Vector2 playerGlobalPosition, Bounds;
        public Boolean followPlayer = true;
        
        public override void _Ready()
        {
            GD.Print("Ready - NavigationAgent and Player set");
            _navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
          
            _player = GetNode<CharacterBody2D>(_playerPath);
            // Set the initial target position for the navigation agent
            Bounds = new Vector2(450, 128);
            playerGlobalPosition = _player.GlobalPosition - Bounds;
            _navigationAgent.PathDesiredDistance = 150;
            if (followPlayer)
            {
                _navigationAgent.SetTargetPosition(playerGlobalPosition);
            }
            _navigationAgent.Radius = 100;
            _navigationAgent.VelocityComputed += _on_nav_velocity_computed;
        }

        public void _on_nav_velocity_computed(Vector2 vel)
        {
            _navigationAgent.SetVelocity(vel);
        }
        
        public override void _PhysicsProcess(double delta)
        {
            Bounds = new Vector2(240,120);//new Vector2(450, 128);
    playerGlobalPosition = _player.GlobalPosition - Bounds;

    if (_navigationAgent != null)
    {
        Vector2 nextPosition = _navigationAgent.GetNextPathPosition();

        // Update navigation as usual
        if (_navigationAgent.IsTargetReached() || _navigationAgent.GetNextPathPosition().DistanceTo(playerGlobalPosition) < _navigationAgent.PathDesiredDistance && _navigationAgent.GetNextPathPosition().DistanceTo(GlobalPosition) < 10)
        {
            ctr++;
            _velocity = Vector2.Zero;
            GD.Print("Reached position within tolerance - stopping. Updated target position to: " + playerGlobalPosition + ". Dist: " + _navigationAgent.PathDesiredDistance);
            if (ctr > 100)
            {
                ctr = 0;
                _navigationAgent.PathDesiredDistance = ran.Next(180, 400);
                GD.Print(_navigationAgent.PathDesiredDistance + " new des. dist");
                if (followPlayer)
                {
                    _navigationAgent.SetTargetPosition(nextPosition);
                }
            }

            GD.Print("Dist: "+GlobalPosition.DistanceTo(playerGlobalPosition));
            
            // if too close back up from player not working yet
            if (playerGlobalPosition.DistanceTo(GlobalPosition) < 100)
            {
               // _velocity = (nextPosition - GlobalPosition).Normalized() * Speed;  // nextPosition switched with GlobalPosition to invert direction
                //Velocity = _velocity;
                //MoveAndSlide();
            }
            //
        }
        else
        {
            _velocity = (playerGlobalPosition - nextPosition).Normalized() * Speed;
            Velocity = _velocity;
            MoveAndSlide();
        }

        // Now update rotation based on direction to player
        //UpdateRotationTowardsPlayer(playerGlobalPosition, delta);
        //LookAt(playerGlobalPosition);
        Sprite2D allieSprite = GetTree().Root.GetNode<Sprite2D>("TileMapLayer/Region/CharacterBody2D/Sprite2D");
        allieSprite.LookAt(playerGlobalPosition);
    }
    else
    {
        GD.Print("NavigationAgent or Player is null.");
    }
}

private void UpdateRotationTowardsPlayer(Vector2 targetPosition, double delta)
{
    float someSmoothingFactor = 3f;
    // Calculate direction
    Vector2 direction = targetPosition - GlobalPosition;

    // Calculate angle in radians
    float angle = Mathf.Atan2(direction.Y, direction.X);

    // Convert to degrees (optional)
    angle = Mathf.RadToDeg(angle);

    // Set rotation smoothly (optional)
    Rotation = Mathf.LerpAngle(Rotation, angle, (float)delta * someSmoothingFactor); // Adjust smoothingFactor for desired speed
}
        
        private Vector2 GetTargetPosition()
        {
            // Replace this with your logic to determine the target position
            // For example, you could use mouse input or AI to calculate the target.
            return _player.GlobalPosition;
        }
    }
}
