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
    [Export] private float _allyMotivationChangeIntervall = 10f;
    [Export] private int _maxMotivationByCore = 5;
    private Map _map = null!;
    private Game.Scripts.Core _core = null!; // Deklaration des Core-Objekts
    private Player _player = null!;
    private static List<MapItem> s_items = null!;
    public static List<MapItem> Items { get => s_items; set => s_items = value; }
    double _timeElapsedHealing = 0f;
    double _timeElapsedMotivation = 0f;
    private int _startItemCount = 34;

    [Export] private int _fleeHealthAmount = 20;

    public override void _Ready()
    {
        _map = this;
        _core = GetNode<Game.Scripts.Core>("%Core");
        _player = GetNode<Player>("%Player");
        s_items = new List<MapItem>();

        // fill item list:
        Material[] materials = (Material[])Enum.GetValues(typeof(Material));
        Random random = new Random();

        for (int i = 0; i < _startItemCount; i++)
        {
            Material randomMaterial = materials[random.Next(materials.Length - 1) + 1];
            int randomX = random.Next(-2000, 2001);
            int randomY = random.Next(-2000, 2001);
            while (randomX is < 700 and > -700)
            {
                randomX = random.Next(-2000, 2001);
            }
            while (randomY is < 700 and > -700)
            {
                randomY = random.Next(-2000, 2001);
            }
            s_items.Add(new MapItem(new Itemstack(randomMaterial), new Location(randomX, randomY)));
        }
    }

    public void MotivationHeal(CharacterBody2D ally){
        if(ally is Ally newAlly){
            if(newAlly.Motivation < _maxMotivationByCore){
                newAlly.Motivation += 1;
            }
        }
        else if(ally is CombatAlly combatAlly){
            if(combatAlly.Motivation < _maxMotivationByCore){
                combatAlly.Motivation += 1;
            }
        }
        _timeElapsedMotivation = 0f;
    }

    public void AllyFlee(CharacterBody2D ally){
        if(ally is Ally newAlly){
            Health hp = newAlly.GetNode<Health>("Health");
            float distanceCore = ally.GlobalPosition.DistanceTo(_core.Position);
            float distancePlayer = ally.GlobalPosition.DistanceTo(_player.Position);
            if(hp.Amount < _fleeHealthAmount && distanceCore < distancePlayer){
                newAlly.PathFindingMovement.TargetPosition = _core.Position;
                newAlly.Fleeing = true;
            }
            else if(hp.Amount < _fleeHealthAmount){
                newAlly.FollowPlayer = true;
                newAlly.Fleeing = true;
            }
            else{
                newAlly.Fleeing = false;
            }
        }
        else if(ally is CombatAlly combatAlly){
            Health hp = combatAlly.GetNode<Health>("Health");
            float distanceCore = ally.GlobalPosition.DistanceTo(_core.Position);
            float distancePlayer = ally.GlobalPosition.DistanceTo(_player.Position);
            if(hp.Amount < _fleeHealthAmount && distanceCore < distancePlayer){
                combatAlly.PathFindingMovement.TargetPosition = _core.Position;
                combatAlly.Fleeing = true;
            }
            else if(hp.Amount < _fleeHealthAmount){
                combatAlly.FollowPlayer = true;
                combatAlly.Fleeing = true;
            }
            else{
                combatAlly.Fleeing = false;
            }
        }
    }

    public void DarknessDamage()
    {
        if (_timeElapsedHealing >= _allyHealthChangeIntervall)
        {
            List<Ally> allyGroup = GetTree().GetNodesInGroup("Entities").OfType<Ally>().ToList();
            List<CombatAlly> combatAllyGroup = GetTree().GetNodesInGroup("Entities").OfType<CombatAlly>().ToList();

            foreach (Ally entity in allyGroup)
            {
                Health hp = entity.GetNode<Health>("Health");
                GD.Print("Ally Motivation: " + entity.Motivation);
                switch (entity.CurrentState)
                {
                    //if ally is in darkness, its health is reduced by 1 point per Intervals
                    case Ally.AllyState.Darkness:
                        hp.Damage(_darknessCircleDamage);
                        break;
                    //if ally is in small circle, it gets 3 health points per Interval
                    case Ally.AllyState.SmallCircle:
                        hp.Heal(_smallCircleHeal);
                        if(_timeElapsedMotivation > _allyMotivationChangeIntervall){
                            MotivationHeal(entity);
                        }
                        break;
                    //if ally is in big circle, it gets 1 health points per Interval
                    case Ally.AllyState.BigCircle:
                        hp.Heal(_bigCircleHeal);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                AllyFlee(entity);
                GD.Print($"{entity.Name} Health: {entity.Health.Amount}");
            }

            foreach (CombatAlly entity in combatAllyGroup)
            {
                GD.Print("combatAllyMotivation: " + entity.Motivation);
                switch (entity.CurrentState)
                {
                    //if ally is in darkness, its health is reduced by 1 point per Intervals
                    case CombatAlly.AllyState.Darkness:
                        entity.Health.Damage(_darknessCircleDamage);
                        break;
                    //if ally is in small circle, it gets 3 health points per Interval
                    case CombatAlly.AllyState.SmallCircle:
                        entity.Health.Heal(_smallCircleHeal); 
                        if(_timeElapsedMotivation > _allyMotivationChangeIntervall){
                            MotivationHeal(entity);
                        }
                        break;
                    //if ally is in big circle, it gets 1 health points per Interval
                    case CombatAlly.AllyState.BigCircle:
                        entity.Health.Heal(_bigCircleHeal);
                        break;
                }
                AllyFlee(entity);
                GD.Print($"{entity.Name} Health: {entity.Health.Amount}");
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
            }
            _timeElapsedHealing = 0;
            if(_timeElapsedMotivation > _allyMotivationChangeIntervall){
                _timeElapsedMotivation = 0f;
            }
        }


    }

    public override void _PhysicsProcess(double delta)
    {
        _timeElapsedHealing += delta;
        _timeElapsedMotivation += delta;
        DarknessDamage();
        _Draw();
        _core.QueueRedraw();
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

        for (int i = 0; i < s_items.Count; i++)
        {
            if (s_items[i].Item.Material == material)
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

        for (int i = 0; i < s_items.Count; i++)
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
