using System;
using System.Collections.Generic;
using System.Linq;

using Game.Scripts;
using Game.Scripts.Items;

using Godot;

using MapItem = Game.Scripts.Items.MapItem;
using Material = Game.Scripts.Items.Material;


public partial class Map : Node2D
{
    [Export] private int _smallCircleHeal = 30;
    [Export] private int _bigCircleHeal = 10;
    [Export] private int _darknessCircleDamage = 30;
    [Export] private float _allyHealthChangeIntervall = 3f;
    private Map _map = null!;
    private Game.Scripts.Core _core = null!; // Deklaration des Core-Objekts
    private Player _player = null!;
    public static List<MapItem> Items { get; private set; } = null!;
    double _timeElapsed = 0f;
    private int _startItemCount = 34;

    public override void _Ready()
    {
        _map = this;
        _core = GetNode<Game.Scripts.Core>("%Core");
        PointLight2D cl = _core.GetNode<PointLight2D>("CoreLight");
        _player = GetNode<Player>("%Player");
        Items = [];

        // fill item list:
        Material[] materials = [Game.Scripts.Items.Material.Copper, // Materials that should be spread out randomly on the map
                                Game.Scripts.Items.Material.Diamond, 
                                Game.Scripts.Items.Material.Gold, 
                                Game.Scripts.Items.Material.Iron, 
                                Game.Scripts.Items.Material.Stone, 
                                Game.Scripts.Items.Material.Wood];
        Random random = new Random();
        for (int i = 0; i < _startItemCount; i++)
        {
            Material randomMaterial = materials[random.Next(materials.Length - 1) + 1];
            int randomX = random.Next(((int) cl.GlobalPosition.X) -2000, ((int) cl.GlobalPosition.X) + 2001);
            int randomY = random.Next(((int) cl.GlobalPosition.Y) -2000, ((int) cl.GlobalPosition.Y) + 2001);

            int xOffsetPos = ((int) cl.GlobalPosition.X) + 700;
            int xOffsetNeg = ((int) cl.GlobalPosition.X) - 700;
            int yOffsetPos = ((int) cl.GlobalPosition.Y) + 700;
            int yOffsetNeg = ((int) cl.GlobalPosition.Y) - 700;

            while (randomX < xOffsetPos && randomX > xOffsetNeg)
            {
                randomX = random.Next(((int) cl.GlobalPosition.X) -2000, ((int) cl.GlobalPosition.X) + 2001);
            }
            while (randomX < yOffsetPos && randomX > yOffsetNeg)
            {
                randomY = random.Next(((int) cl.GlobalPosition.Y) -2000, ((int) cl.GlobalPosition.Y) + 2001);
            }
            Items.Add(new MapItem(new Itemstack(randomMaterial), new Location(randomX, randomY)));
        }
    }

    private void DarknessDamage()
    {

        if (_timeElapsed >= _allyHealthChangeIntervall)
        {
            List<Ally> allyGroup = GetTree().GetNodesInGroup("Entities").OfType<Ally>().ToList();
            List<CombatAlly> combatAllyGroup = GetTree().GetNodesInGroup("Entities").OfType<CombatAlly>().ToList();

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

               // int($"{entity.Name} Health: {entity.Health.Amount}");
            }

            Health playerhp = _player.GetNode<Health>("Health");
            switch (_player.CurrentState)
            {
                //if ally is in darkness, its health is reduced by 1 point per Intervals
                case Player.AllyState.Darkness:
                    playerhp.Damage(_darknessCircleDamage);
                    break;
                //if ally is in small circle, it gets 3 health points per Interval
                case Player.AllyState.SmallCircle:
                    playerhp.Heal(_smallCircleHeal);
                    break;
                //if ally is in big circle, it gets 1 health points per Interval
                case Player.AllyState.BigCircle:
                    playerhp.Heal(_bigCircleHeal);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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

    public void AddItem(Itemstack item, int x, int y)
    {
        Items.Add(new MapItem(item, x, y));
    }

    public List<MapItem> GetItems()
    {
        return Items;
    }

    private static List<Location> GetAllMaterialLocations(Material material)
    {
        return (from t in Items where t.Item.Material == material select t.Location).ToList();
    }

    private static List<Location> GetAllItemLocations()
    {
        List<Location> locations = new List<Location>();

        foreach (Location loc in Items.Select(item => item.Location))
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
