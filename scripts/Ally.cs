using System;
using System.Text.RegularExpressions;
using Game.Scripts;
using Game.Scripts.Items;

using Godot;

public partial class Ally : CharacterBody2D
{
    public Health Health = null!;
    [Export] Chat _chat = null!;
    [Export] RichTextLabel _responseField = null!;
    [Export] PathFindingMovement _pathFindingMovement = null!;
    [Export] private Label _nameLabel = null!;
    private bool _followPlayer = true;
    private bool _busy;
    private bool _reached;
    private bool _harvest;
    private bool _returning;
    private int _motivation;
    private static Inventory _inventory = new Inventory(36);
    private Player _player = null!;

    //Enum with states for ally in darkness, in bigger or smaller circle for map damage system
    public enum AllyState
    {
        Darkness,
        SmallCircle,
        BigCircle
    }

    public AllyState CurrentState { get; private set; } = AllyState.SmallCircle;

    private Game.Scripts.Core _core = null!;

    public override void _Ready()
    {
        Health = GetNode<Health>("Health");
        _chat.ResponseReceived += HandleResponse;
        _player = GetNode<Player>("%Player");

        _core = GetNode<Game.Scripts.Core>("%Core");
        //GD.Print($"Path to Chat: {_chat.GetPath()}");
        //GD.Print($"Path to ResponseField: {_responseField.GetPath()}");
        //GD.Print($"Path to PathFindingMovement: {_pathFindingMovement.GetPath()}");
    }

    public void SetAllyInDarkness()
    {
        // Berechne den Abstand zwischen Ally und Core
        Vector2 distance = this.Position - _core.Position;
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

    public override void _PhysicsProcess(double delta)
    {
        //Check where ally is (darkness, bigger, smaller)
        SetAllyInDarkness();

        UpdateTarget();
        
        if(GlobalPosition.DistanceTo(_pathFindingMovement.TargetPosition) < 300)
        {
            _reached = true;
        }
        else
        {
            _reached = false;
        }

        
        if (_harvest && _reached) // Harvest logic
        {
            Harvest();
        }
    }

    private void UpdateTarget()
    {
        if (_followPlayer && !_busy)
        {
            _pathFindingMovement.TargetPosition = _player.GlobalPosition;
        }
        
        

        if (_harvest)
        {
            if (_returning)
            {
                PointLight2D cl = _core.GetNode<PointLight2D>("CoreLight");
                Vector2 targ = new Vector2(0, 500);  // cl.GlobalPosition;
                // Target = core
                _pathFindingMovement.TargetPosition = targ; //_core.GlobalPosition;
                //GD.Print("Target position (should be CORE): " + _pathFindingMovement.TargetPosition.ToString());
            }
            else
            {
                Location nearestLocation = Map.GetNearestItemLocation(new Location(GlobalPosition))!;
                
                //GD.Print("going to nearest loc("+nearestLocation.X +", "+nearestLocation.Y+") from "+ GlobalPosition.X + " " + GlobalPosition.Y);
                //Target = nearest item
                _pathFindingMovement.TargetPosition = nearestLocation.toVector2();

            }
        }
    }

    private void HandleResponse(string response)
    {

        _responseField.Text = response;

        GD.Print($"Response: {response}");

        string pattern = @"MOTIVATION:\s*(\d+)";
        Regex regex = new Regex(pattern);
        Match match = regex.Match(response);

        if (match is { Success: true, Groups.Count: > 1 })
        {
            try
            {
                _motivation = int.Parse(match.Groups[1].Value);
            }
            catch (Exception ex)
            {
                GD.Print(ex);
            }
        }

        if (response.Contains("FOLLOW"))
        {
            GD.Print("following");
            _followPlayer = true;
        }

        if (response.Contains("STOP"))
        {
            GD.Print("stop");
            _followPlayer = false;
        }

        if (response.Contains("HARVEST") && !_busy)
        {
            GD.Print("harvesting");
            if (Map.Items.Count > 0)
            {
                _harvest = true; // Change harvest state
                _busy = true; // Change busy state
            }
        }

        GD.Print($"Motivation: {_motivation}");
    }
    
    private void Harvest()
    {
        if (!_returning)
        {
            // extract the nearest item and add to inventory (pickup)
            if (_inventory.HasSpace()) // if inventory has space
            {
                GD.Print("harvesting...");
                Itemstack item = Map.ExtractNearestItemAtLocation(new Location(GlobalPosition));
                GD.Print(item.Material+ " amount: "+item.Amount);
                _inventory.AddItem(item); // add item to inventory
                _inventory.Print();
            } // if inventory has no space don't harvest it
            else
            {
                GD.Print("No space");
            }

            _returning = true;
        }
        else
        {
            // Empty inventory into the core
            
            foreach (Itemstack item in _inventory.GetItems())
            {
                if (item.Material == Game.Scripts.Items.Material.None)
                {
                    continue;
                }
                _core.MaterialCount += item.Amount;
                _core.IncreaseScale();
                GD.Print("Increased scale");
            }
            _inventory.Clear();
            _busy = false; // Change busy state  
            _harvest = false; // Change harvest state
            _returning = false; // Change returning state
        }
    }
}
