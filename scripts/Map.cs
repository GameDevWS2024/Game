using Godot;
using System;
using Game.Scripts;


public partial class Map : Node2D
{
	private Map _map;
	private Core core; // Deklaration des Core-Objekts
	private Player _player;
	private Ally _ally;
	private Health _allyHealth;
	static double _timeElapsed = 0f;
	private float _allyHealthInterval = 3f;

	public override void _Ready()
	{
		_map = this;
		core = GetNode<Core>("%Core"); // Initialisierung in _Ready
		GD.Print(core.Scale); // Ausgabe der Skalierung in der Godot-Konsole
		_player = GetNode<Player>("%Player");
		_ally = GetNode<Ally>("%Ally");
		_allyHealth = _ally.GetNode<Health>("Health");
	}
	
	public void _DarknessDamage() {
		//if ally is in darkness, its health is reduced by 1 point per Intervals
		if(_ally._allyInDarkness && (_allyHealth.Amount >= 0) && (_timeElapsed >= _allyHealthInterval)) {
			_allyHealth.Damage(3);
			_timeElapsed = 0f;
			}
			
		//if ally is in small circle, it gets 3 health points per Interval
		else if(_ally._allyInSmallCircle && (_allyHealth.Amount < 100) && (_timeElapsed >= _allyHealthInterval)) {
			_allyHealth.Heal(3);
			if(_allyHealth.Amount > 100){
				_allyHealth.Damage(_allyHealth.Amount % 100);
			}
			_timeElapsed = 0f;
		}
		
		//if ally is in big circle, it gets 1 health points per Interval
		else if(_ally._allyInBigCircle && (_allyHealth.Amount < 100) && (_timeElapsed >= _allyHealthInterval)) {
			_allyHealth.Heal(1);
			_timeElapsed = 0f;
		}
	}
	
	public override void _PhysicsProcess(double delta)
	{
		_timeElapsed += delta;
		_DarknessDamage();
		_Draw();
	}
}
