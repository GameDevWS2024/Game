using System;
using System.Collections.Generic;
using System.Linq;

using Game.Scripts;
using Game.Scripts.Items;

using Godot;

using Material = Game.Scripts.Items.Material;


public partial class Map : Node2D
{
    [Export] private int _smallCircleHeal = 30;
    [Export] private int _bigCircleHeal = 10;
    [Export] private int _darknessCircleDamage = 30;
    [Export] private float _allyHealthChangeIntervall = 3f;
    private Map _map = null!;
    private Core _core = null!; // Deklaration des Core-Objekts
    private Player _player = null!;
    private static List<MapItem> s_items = null!;
    public static List<MapItem> Items { get => s_items; set => s_items = value; }
    double _timeElapsed = 0f;
    private int _startItemCount = 34;

    public override void _Ready()
    {
        _map = this;
        _core = GetNode<Core>("%Core");
        _player = GetNode<Player>("%Player");
        s_items = new List<MapItem>();
        
        // fill item list:
        Material[] materials = (Material[])Enum.GetValues(typeof(Material));
        Random random = new Random();

        for(int i = 0; i < _startItemCount; i++)
        {
            Material randomMaterial = materials[random.Next(materials.Length)];
            int randomX = random.Next(-2000, 2001);
            int randomY = random.Next(-2000, 2001);
            s_items.Add(new MapItem(new Itemstack(randomMaterial), new Location(randomX, randomY)));
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

    public void AddItem(Itemstack item, int x, int y)
    {
        s_items.Add(new MapItem(item, x, y));
    }

    public List<MapItem> GetItems()
    {
        return s_items;
    }

    public List<Location> GetAllMaterialLocations(Material material)
    {
        List<Location> locations = new List<Location>();

        for(int i = 0; i < s_items.Count; i++)
        {
            if(s_items[i].Item.Material == material)
            {
                locations.Add(s_items[i].Location);
            }
        }

        return locations;
    }
    
    public static List<Location> GetAllItemLocations()
    {
        List<Location> locations = new List<Location>();

        foreach (Location loc in s_items.Select(item => item.Location))
        {
            locations.Add(loc);
        }

        return locations;
    }

    public static Location? GetNearestItemLocation(Location loc)
    {
        return GetAllItemLocations().OrderBy(itemLoc => itemLoc.DistanceTo(loc)).FirstOrDefault();
    }
    
    public Location? GetNearestMaterialLocation(Location loc, Material mat)
    {
        return GetAllMaterialLocations(mat).OrderBy(itemLoc => itemLoc.DistanceTo(loc)).FirstOrDefault();
    }

    public static Itemstack ExtractNearestItemAtLocation(Location loc)
    {
        float nearest = float.MaxValue;
        int index = -1;
        
        for(int i = 0; i < s_items.Count; i++)
        {
            float distance = loc.DistanceTo(s_items[i].Location);
            if (distance > nearest) { continue; }
            nearest = distance;
            index = i;
        }

        if (index < 0) { return new Itemstack(Game.Scripts.Items.Material.None); }
        
        Itemstack item = s_items[index].Item;
        s_items.RemoveAt(index);
        return item;
    }
}
