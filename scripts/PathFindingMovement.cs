using Godot;
public partial class PathFindingMovement : Node
{
    [Signal] public delegate void ReachedTargetEventHandler();

    [Export] private int _minTargetDistance = 300;
    [Export] private int _targetDistanceVariation = 50;
    [Export] private int _speed = 250;
    [Export] bool _debug = false;

    [Export] CharacterBody2D _character;
    [Export] NavigationAgent2D _agent;
    [Export] Sprite2D _sprite;

    public Vector2 TargetPosition { get; set; }

    private bool _reachedTarget;
    private int _currentTargetDistance;

    public override void _Ready()
    {
        _currentTargetDistance = _minTargetDistance;
    }

    public override void _PhysicsProcess(double delta)
    {
        _agent.SetTargetPosition(TargetPosition);

        if (_debug)
        {
            float distance = _character.GlobalPosition.DistanceTo(_agent.TargetPosition);
            GD.Print($"Distance: {distance}, Target Position: {_agent.TargetPosition}");
        }

        if (_character.GlobalPosition.DistanceTo(_agent.TargetPosition) > _currentTargetDistance)
        {
            _reachedTarget = false;
            Vector2 currentLocation = _character.GlobalPosition;
            Vector2 nextLocation = _agent.GetNextPathPosition();
            Vector2 newVel = (nextLocation - currentLocation).Normalized() * _speed;

            if (newVel.X != 0 && _sprite != null)
            {
                _sprite.FlipH = newVel.X > 0;
            }

            _character.Velocity = newVel;
            _character.MoveAndSlide();
        }
        else if (!_reachedTarget)
        {
            _currentTargetDistance = GD.RandRange(_minTargetDistance - _targetDistanceVariation / 2,
                                                _minTargetDistance + _targetDistanceVariation / 2);
            EmitSignal(SignalName.ReachedTarget);
            _reachedTarget = true;
        }
    }
}
