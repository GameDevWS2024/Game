using System;
using System.Collections.Generic;
using System.Linq;

using Game.Scripts;
using Game.Scripts.Items;

using Godot;

public partial class CircleLogic : Node
{
    [Export] CharacterBody2D _character;
    private Core _core = null!;
    double _timeElapsed = 0f;
    [Export] private int _smallCircleHeal = 30;
    [Export] private int _bigCircleHeal = 10;
    [Export] private int _darknessCircleDamage = 30;
    [Export] private float _allyHealthChangeIntervall = 3f;
    private Map _map = null!;
    private static readonly List<MapItem> s_items = null!;

    public AllyState CurrentState { get; private set; } = AllyState.SmallCircle;
    //Enum with states for ally in darkness, in bigger or smaller circle for map damage system
    public enum AllyState
    {
        Darkness,
        SmallCircle,
        BigCircle
    }

    public void SetAlliesInDarkness()
    {
        // Berechne den Abstand zwischen Ally und Core
        Vector2 distance = _character.Position - _core.Position;
        float distanceLength = distance.Length();  // Berechne die Länge des Vektors

        // If ally further away than big circle, he is in the darkness
        if (distanceLength > _core.LightRadiusBiggerCircle)
        {
            CurrentState = AllyState.Darkness;
        }
        //if ally not in darkness and closer than the small Light Radius, he is in small circle
        else if (distanceLength < _core.LightRadiusSmallerCircle)
        {
            CurrentState = AllyState.SmallCircle;
        }
        //if ally not in darkness and not in small circle, ally is in big circle
        else
        {
            CurrentState = AllyState.BigCircle;
        }

    }

    public void DarknessDamage()
    {
        if (_timeElapsed >= _allyHealthChangeIntervall)
        {
            Health hp = _character.GetNode<Health>("Health");
            switch (CurrentState)
            {
                //if ally is in darkness, its health is reduced by 1 point per Intervals
                case AllyState.Darkness:
                    hp.Damage(_darknessCircleDamage);
                    break;
                //if ally is in small circle, it gets 3 health points per Interval
                case AllyState.SmallCircle:
                    hp.Heal(_smallCircleHeal);
                    break;
                //if ally is in big circle, it gets 1 health points per Interval
                case AllyState.BigCircle:
                    hp.Heal(_bigCircleHeal);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            GD.Print($"{_character.Name} Health: {hp}");
            _timeElapsed = 0;
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        GD.Print($"HEELO{_character.Position}");
        _core = GetNode<Core>("%Core");
        GD.Print(GetNode<Core>("%Core"));
        GD.Print(GetNode<Core>("%Core").Name);
        GD.Print($"HEELO{_core.Position}");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        _timeElapsed += delta;
        SetAlliesInDarkness();
        DarknessDamage();
    }
}
