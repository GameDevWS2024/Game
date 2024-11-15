using System;

using Godot;

namespace Game.Scripts;

public partial class Ally : CharacterBody2D
{
    [Export] private NodePath _playerPath;
    private CharacterBody2D _player;
    private NavigationAgent2D _navigationAgent;
    private const float Speed = 150.0f;
    private Vector2 _velocity = Vector2.Zero;
    private const double Accel = 2;
    private readonly Random _ran = new Random();
    private int _ctr = 0;
    private Vector2 _playerGlobalPosition, _bounds, _targetPosition;
    private Boolean _followPlayer = false, _debugPrint = false;

    public Ally(NodePath playerPath)
    {
        _playerPath = playerPath;
    }

    public void GoTo(Vector2 loc)
    {
        _targetPosition = loc;
        SetFollowPlayer(false);
        while (!_navigationAgent.IsTargetReached() || GlobalPosition.DistanceTo(_navigationAgent.GetTargetPosition()) < 50)
        {
        }
        DecideWhatNow();
    }

    private void DecideWhatNow()
    {
        if (_followPlayer)
        {
            // ...  
        }
        // else if (ENEMIES NEARBY) ...
        // else if (ALREADY BUSY)
        // else if (RANDOM CHANCE)
        // else if (TOO FAR FROM PLAYER)
        else
        {
            SetFollowPlayer(true);
        }
    }
    public void SetFollowPlayer(Boolean follow)
    {
        _followPlayer = follow;
    }

    public void SwitchFollowPlayer()
    {
        _followPlayer = !_followPlayer;
    }
    public Boolean GetFollowPlayer()
    {
        return _followPlayer;
    }

    public override void _Ready()
    {
        _navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        _player = GetNode<CharacterBody2D>(_playerPath);
        _bounds = new Vector2(450, 128);
        _playerGlobalPosition = _player.GlobalPosition - _bounds;
        _navigationAgent.PathDesiredDistance = 150;
        _navigationAgent.Radius = 100;
        if (_followPlayer)
        {
            _navigationAgent.SetTargetPosition(_playerGlobalPosition);
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        _bounds = new Vector2(240, 120); //new Vector2(450, 128);
        _playerGlobalPosition = _player.GlobalPosition - _bounds;
        Vector2 nextPosition = _navigationAgent.GetNextPathPosition();

        // Update navigation as usual
        if (_navigationAgent.IsTargetReached() ||
            _navigationAgent.GetNextPathPosition().DistanceTo(_playerGlobalPosition) <
            _navigationAgent.PathDesiredDistance &&
            _navigationAgent.GetNextPathPosition().DistanceTo(GlobalPosition) < 10)
        {
            _ctr++;
            _velocity = Vector2.Zero;
            if (_debugPrint)
            {
                GD.Print("Reached position within tolerance - stopping. Updated target position to: " + _playerGlobalPosition + ". Dist: " + _navigationAgent.PathDesiredDistance);
            }

            if (_ctr > 100)
            {
                _ctr = 0;
                _navigationAgent.PathDesiredDistance = _ran.Next(180, 400);
                if (_debugPrint)
                {
                    GD.Print(_navigationAgent.PathDesiredDistance + " new des. dist");
                }
                if (_followPlayer)
                {
                    _navigationAgent.SetTargetPosition(nextPosition);
                }
            }

            if (_debugPrint)
            {
                GD.Print("Dist: " + GlobalPosition.DistanceTo(_playerGlobalPosition));
            }

            // if too close back up from player not working yet
            if (_playerGlobalPosition.DistanceTo(GlobalPosition) < 100)
            {
                // _velocity = (nextPosition - GlobalPosition).Normalized() * Speed;  // nextPosition switched with GlobalPosition to invert direction
                //Velocity = _velocity;
                //MoveAndSlide();
            }
            //
        }
        else
        {
            _velocity = (_playerGlobalPosition - nextPosition).Normalized() * Speed;
            Velocity = _velocity;
            MoveAndSlide();
        }

        //// Now update rotation based on direction to player
        // Sprite2D allieSprite = GetTree().Root.GetNode<Sprite2D>("TileMapLayer/Region/MeshInstance2D/CharacterBody2D/Sprite2D");
        // allieSprite.LookAt(_playerGlobalPosition);
        //GD.Print(allieSprite.GlobalScale);
    }
    private void UpdateRotationTowardsPlayer(Vector2 targetPosition, double delta)
    {
        float someSmoothingFactor = 3f;
        Vector2 direction = targetPosition - GlobalPosition;
        // Calculate angle in radians
        float angle = Mathf.Atan2(direction.Y, direction.X);
        // Convert to degrees
        angle = Mathf.RadToDeg(angle);
        // Set rotation smoothly (optional)
        Rotation = Mathf.LerpAngle(Rotation, angle, (float)delta * someSmoothingFactor); // Adjust smoothingFactor for desired speed
    }
    private Vector2 GetTargetPosition()
    {
        // you could use mouse input or AI to calculate the target.
        return _targetPosition;
    }
}
