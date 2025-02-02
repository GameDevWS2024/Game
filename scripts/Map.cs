using System;
using System.Collections.Generic;
using System.Linq;

using Game.Scripts;
using Game.Scripts.Items;

using Godot;

using MapItem = Game.Scripts.Items.MapItem;
using Material = Game.Scripts.Items.Material;


namespace Game.Scripts;

public partial class Map : Node2D
{
    [Export] private int _smallCircleHeal = 30;
    [Export] private int _bigCircleHeal = 10;
    [Export] private int _darknessCircleDamage = 30;
    [Export] private float _allyHealthChangeIntervall = 3f;
    private Game.Scripts.Core _core = null!;

    public static List<MapItem> Items { get; private set; } = null!;
    double _timeElapsed = 0f;
    private int _startItemCount = 34;

    public override void _Ready()
    {
        _core = GetNode<Game.Scripts.Core>("%Core");
        Items = [];


        // fill item list:
        Material[] materials = (Material[])Enum.GetValues(typeof(Material));
        Random random = new Random();

        for (int i = 0; i < _startItemCount; i++)
        {
            Material randomMaterial = materials[random.Next(materials.Length - 1) + 1];
            int randomX = random.Next(-2000, 2001), randomY = random.Next(-2000, 2001);
            while (randomX is < 700 and > -700)
            {
                randomX = random.Next(-2000, 2001);
            }
            while (randomY is < 700 and > -700)
            {
                randomY = random.Next(-2000, 2001);
            }
            Items.Add(new MapItem(new Itemstack(randomMaterial), new Location(randomX, randomY)));
        }
    }

    private void DarknessDamage()
    {

        if (_timeElapsed >= _allyHealthChangeIntervall)
        {
            List<Game.Scripts.Ally> allyGroup = GetTree().GetNodesInGroup("Entities").OfType<Ally>().ToList();

            foreach (Ally entity in allyGroup)
            {
                Health hp = entity.GetNode<Health>("Health");
                switch (entity.CurrentState)
                {
                    //if ally is in darkness, its health is reduced by 1 point per Intervals
                    case Ally.AllyState.Darkness:
                        hp.Damage(_darknessCircleDamage);
                        break;
                    //if ally is in small circle, it gets 3 health points per Interval
                    case Ally.AllyState.SmallCircle:
                        hp.Heal(_smallCircleHeal);
                        break;
                    //if ally is in big circle, it gets 1 health points per Interval
                    case Ally.AllyState.BigCircle:
                        hp.Heal(_bigCircleHeal);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // GD.Print($"{entity.Name} Health: {entity.Health.Amount}");
            }
            _timeElapsed = 0;
        }


    }

    public override void _PhysicsProcess(double delta)
    {
        _timeElapsed += delta;
        DarknessDamage();
        _Draw();
        _core.QueueRedraw();
    }

    public static void AddItem(Itemstack item, int x, int y)
    {
        Items.Add(new MapItem(item, x, y));
    }

    public static List<MapItem> GetItems()
    {
        return Items;
    }

    private static List<Location> GetAllMaterialLocations(Material material)
    {
        return (from t in Items where t.Item.Material == material select t.Location).ToList();
    }

    private static List<Location> GetAllItemLocations()
    {
        return Items.Select(item => item.Location).ToList();
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

        for (int i = 0; i < Items.Count; i++)
        {
            float distance = loc.DistanceTo(Items[i].Location);
            if (distance > nearest) { continue; }
            nearest = distance;
            index = i;
        }

        if (index < 0) { return new Itemstack(Game.Scripts.Items.Material.None); }

        Itemstack item = Items[index].Item;
        Items.RemoveAt(index);
        return item;
    }
}
