using Godot;

public partial class Ally : CharacterBody2D
    {
    
    [Export] private int _minTargetDistance = 300;
    [Export] private int _targetDistanceVariation = 50;
    [Export] private int _speed = 250; 

    public bool ReachedTarget {get; private set;}
    private bool FollowPlayer {get; set;} = true;
    private NavigationAgent2D _agent;
    private CharacterBody2D _player;
    private Sprite2D _sprite; 

    private int _currentTargetDistance;
    
    public override void _Ready()
    {
        _agent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        _player = GetNode<CharacterBody2D>("%Player");
        _sprite = GetNode<Sprite2D>("Sprite2D"); 
        _currentTargetDistance = _minTargetDistance;
    }

    

    public override void _Process(double delta)
    {
        if (_player == null)
        {
            return;
        }

        if(FollowPlayer) {
            _agent.SetTargetPosition(_player.GlobalPosition);
        }
        
        if (GlobalPosition.DistanceTo(_agent.TargetPosition) > _currentTargetDistance)
        {
            ReachedTarget = false;
            Vector2 curLoc = GlobalPosition;
            Vector2 nextLoc = _agent.GetNextPathPosition();
            Vector2 newVel = (nextLoc - curLoc).Normalized() * _speed;

           if (newVel.X != 0){
            _sprite.FlipH = newVel.X > 0;
           }

            Velocity = newVel;
            MoveAndSlide();
        }

        else if (!ReachedTarget){
            _currentTargetDistance = GD.RandRange(_minTargetDistance - _targetDistanceVariation/2, _minTargetDistance + _targetDistanceVariation/2);
            ReachedTarget = true;
        }
    }

    public void GoTo(Vector2 target) {
        FollowPlayer = false;
        _agent.SetTargetPosition(target);
    }

    private void OnDetectFollowPlayerInstruction(bool follow, int speed) {
        FollowPlayer = follow;
        _speed = speed;
    } 
}