using System;
using System.Collections.Generic;
using System.Linq;

using Game.Scripts;

using Godot;


public partial class Map : Node2D
{
    private Map _map = null!;
    private Core _core = null!; // Deklaration des Core-Objekts
    private Player _player = null!;
    private Ally _ally = null!;
    private Health _allyHealth = null!;
    static double s_timeElapsed = 0f;
    private float _allyHealthInterval = 3f;
    private List<Ally> _entityGroup = null!;

    public override void _Ready()
    {
        _map = this;
        _core = GetNode<Core>("%Core");
        _player = GetNode<Player>("%Player");
        _entityGroup = GetTree().GetNodesInGroup("Entities").OfType<Ally>().ToList();
    }

    public void DarknessDamage()
    {
        foreach (Ally entity in _entityGroup)
        {
            if (s_timeElapsed >= _allyHealthInterval)
            {
                switch (entity.CurrentState)
                {
                    //if ally is in darkness, its health is reduced by 1 point per Intervals
                    case Ally.AllyState.Darkness:
                        entity.Health.Damage(30);
                        break;
                    //if ally is in small circle, it gets 3 health points per Interval
                    case Ally.AllyState.SmallCircle:
                        entity.Health.Heal(30);
                        break;
                    //if ally is in big circle, it gets 1 health points per Interval
                    case Ally.AllyState.BigCircle:
                        entity.Health.Heal(10);
                        break;
                }

            }
            GD.Print($"{entity.Name} Health: {entity.Health.Amount}");
        }
        if (s_timeElapsed >= _allyHealthInterval)
        {
            s_timeElapsed = 0.0f;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        s_timeElapsed += delta;
        DarknessDamage();
        _Draw();
    }
}
