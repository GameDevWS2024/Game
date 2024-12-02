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
    private Core _core = null!; // Deklaration des Core-Objekts
    private Player _player = null!;
    private List<MapItem> _items;
    double _timeElapsed = 0f;
    private int _startItemCount = 30;

    public override void _Ready()
    {
        _map = this;
        _core = GetNode<Core>("%Core");
        _player = GetNode<Player>("%Player");
        _items = new List<MapItem>();
        
        //fill item list:
        Material[] materials = (Material[])Enum.GetValues(typeof(Material));
        Random random = new Random();

        for(int i = 0; i < _startItemCount; i++) {
            _items.AddItem(new MapItem(materials[random.Next(materials.Length-1) + 1]))
        }
    }

    public void DarknessDamage()
    {

        if (_timeElapsed >= _allyHealthChangeIntervall)
        {
            List<Ally> entityGroup = GetTree().GetNodesInGroup("Entities").OfType<Ally>().ToList();
            foreach (Ally entity in entityGroup)
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
            _timeElapsed = 0;
        }


    }

    public override void _PhysicsProcess(double delta)
    {
        _timeElapsed += delta;
        DarknessDamage();
        _Draw();
    }

    public void AddItem(ItemStack item, int x, int y)
    {
        _items.Add(new MapItem(item, x, y));
    }

    public List<MapItem> GetItems()
    {
        return _items;
    }

    public List<Location> GetAllItem(Material material)
    {
        List<Location> locations = new List<Location>;

        for(int i = 0; i < _items.size; i++)
        {
            if(_items[i].Item.Material == material)
            {
                locations.AddItem(new Location(_items[i].X, _items[i].Y));
            }
        }
    }
}
