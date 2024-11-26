using Godot;
using System;
using Game.Scripts;


public partial class Map : Node2D
{
	private Map _map;
	private Core core; // Deklaration des Core-Objekts
	private Player _player;
	private Ally _ally;

	public override void _Ready()
	{
		_map = this;
		core = GetNode<Core>("%Core"); // Initialisierung in _Ready
		GD.Print(core.Scale); // Ausgabe der Skalierung in der Godot-Konsole
		_player = GetNode<Player>("%Player");
		_ally = GetNode<Ally>("%Ally");
	}
	
	
	double _timeElapsed = 0f;
	private float _allyHealthInterval = 3f;
	
	public override void _PhysicsProcess(double delta)
	{
		_timeElapsed += delta;
		//if ally is in darkness, its health is reduced by 1 point per Intervals
		if(_ally._allyInDarkness && (_ally._healthPoints >= 0) && (_timeElapsed >= _allyHealthInterval)) {
			_ally._healthPoints -= 1;
			GD.Print(_ally._healthPoints);
			_timeElapsed = 0f;
		}
		//if ally is in small circle, it gets 3 health points per Interval
		else if(_ally._allyInSmallCircle && (_ally._healthPoints < 100) && (_timeElapsed >= _allyHealthInterval)) {
			_ally._healthPoints += 3;
			if(_ally._healthPoints > 100){
				_ally._healthPoints = 100;
			}
			GD.Print(_ally._healthPoints);
			_timeElapsed = 0f;
		}
		//if ally is in big circle, it gets 1 health points per Interval
		else if(_ally._allyInBigCircle && (_ally._healthPoints < 100) && (_timeElapsed >= _allyHealthInterval)) {
			_ally._healthPoints += 1;
			GD.Print(_ally._healthPoints);
			_timeElapsed = 0f;
		}
		
		_Draw();
	}
}
