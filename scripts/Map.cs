using System;
using System.Collections.Generic;
using System.Linq;

using Game.Scripts;

using Godot;


public partial class Map : Node2D
{
    [Export] private int _smallCircleHeal = 30;
    [Export] private int _bigCircleHeal = 10;
    [Export] private int _darknessCircleDamage = 30;
    [Export] private float _allyHealthChangeIntervall = 3f;
    private Map _map = null!;
    private Game.Scripts.Core _core = null!; // Deklaration des Core-Objekts
    private Player _player = null!;
    double _timeElapsed = 0f;

    public override void _Ready()
    {
        _map = this;
        _core = GetNode<Game.Scripts.Core>("%Core");
        _player = GetNode<Player>("%Player");
    }

    public void DarknessDamage()
    {

        if (_timeElapsed >= _allyHealthChangeIntervall)
        {
            List<Ally> allyGroup = GetTree().GetNodesInGroup("Entities").OfType<Ally>().ToList();
            List<CombatAlly> combatAllyGroup = GetTree().GetNodesInGroup("Entities").OfType<CombatAlly>().ToList();

            foreach (Ally entity in allyGroup)
            {
                switch (entity.CurrentState)
                {
                    //if ally is in darkness, its health is reduced by 1 point per Intervals
                    case Ally.AllyState.Darkness:
                        entity.Health.Damage(_darknessCircleDamage);
                        break;
                    //if ally is in small circle, it gets 3 health points per Interval
                    case Ally.AllyState.SmallCircle:
                        entity.Health.Heal(_smallCircleHeal);
                        break;
                    //if ally is in big circle, it gets 1 health points per Interval
                    case Ally.AllyState.BigCircle:
                        entity.Health.Heal(_bigCircleHeal);
                        break;
                }

                GD.Print($"{entity.Name} Health: {entity.Health.Amount}");
            }

            foreach (CombatAlly entity in combatAllyGroup)
            {
                switch (entity.CurrentState)
                {
                    //if ally is in darkness, its health is reduced by 1 point per Intervals
                    case CombatAlly.AllyState.Darkness:
                        entity.Health.Damage(_darknessCircleDamage);
                        break;
                    //if ally is in small circle, it gets 3 health points per Interval
                    case CombatAlly.AllyState.SmallCircle:
                        entity.Health.Heal(_smallCircleHeal);
                        break;
                    //if ally is in big circle, it gets 1 health points per Interval
                    case CombatAlly.AllyState.BigCircle:
                        entity.Health.Heal(_bigCircleHeal);
                        break;
                }

                GD.Print($"{entity.Name} Health: {entity.Health.Amount}");
            }
            _timeElapsed = 0;
        }


    }

    public override void _PhysicsProcess(double delta)
    {
        _timeElapsed += delta;
        DarknessDamage();
        _Draw();
    }
}
