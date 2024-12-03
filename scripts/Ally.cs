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

    private Core _core = null!;

    public override void _Ready()
    {
        Health = GetNode<Health>("Health");
        _chat.ResponseReceived += HandleResponse;
        _player = GetNode<Player>("%Player");
        _core = GetNode<Core>("%Core");
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

        if (_followPlayer)
        {
            _pathFindingMovement.TargetPosition = _player.GlobalPosition;
        }
    }

    private void HandleResponse(string response)
    {

        _responseField.Text = response;

        GD.Print($"Response: {response}");

        string pattern = @"MOTIVATION:\s*(\d+)";
        Regex regex = new Regex(pattern);
        Match match = regex.Match(response);

        if (match.Success && match.Groups.Count > 1)
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
                Location nearestLocation = Map.GetNearestItemLocation(new Location(GlobalPosition))!;
                /////
                GD.Print ("nearest location: " + nearestLocation.toVector2().X+", "+nearestLocation.toVector2().Y);
                //
                if (nearestLocation == null)
                {
                    GD.Print("no nearest location");
                    return;
                }
                // goto
               // _pathFindingMovement.GoTo(nearestLocation.toVector2());
                //
                _busy = true; // Change busy state
                GD.Print("going to ");
                //Go to nearest item
                _pathFindingMovement.TargetPosition = nearestLocation.toVector2();
              //  GD.Print(_pathFindingMovement.GetParent<Node2D>().GlobalPosition.DistanceTo(nearestLocation.toVector2()));
              //  while (GlobalPosition.DistanceTo(nearestLocation.toVector2()) > 150)
               // {
                    // Do nothing while walking
                 //   GD.Print("Waiting for target");
               // }
                GD.Print("reached");
                // extract the nearest item and add to inventory (pickup)
                //Inventory inv = GetNode<Inventory>("Inventory");
                if (_inventory.HasSpace()) // if inventory has space
                {
                    GD.Print("Space");
                    Itemstack item = Map.ExtractNearestItemAtLocation(new Location(GlobalPosition));
                    GD.Print(item.Material+ " amount: "+item.Amount);
                    _inventory.AddItem(item); // add item to inventory
                } // if inventory has no space don't harvest it
                else
                {
                    GD.Print("No space");
                }

                // Go back to core
                PointLight2D cl = _core.GetNode<PointLight2D>("CoreLight");
                Vector2 targ = new Vector2(500, 700);  // cl.GlobalPosition;
                _pathFindingMovement.GoTo(targ);
               // _pathFindingMovement.TargetPosition = targ; //_core.GlobalPosition;
                GD.Print("Target position (should be CORE): " + _pathFindingMovement.TargetPosition.ToString());
            //    while (!_pathFindingMovement.HasreachedTarget())
              //  {
                    // Do nothing while walking
                //    GD.Print("Walking back to core");
              //  }
                
                // Empty inventory into the core

                foreach (Itemstack item in _inventory.GetItems())
                {
                    if (item.Material == Game.Scripts.Items.Material.None)
                    {
                        continue;
                    }
                    GD.Print("increasing "+item.ToString()+" by "+item.Amount);
                    _core.MaterialCount += item.Amount;
                    GD.Print("core has materials:"+_core.MaterialCount);
                    _core.IncreaseScale();
                    GD.Print("Increased scale");
                }
                _inventory.Clear();
                
                // Go back to player
                _pathFindingMovement.TargetPosition = _player.GlobalPosition;
                while (!_pathFindingMovement.HasreachedTarget())
                {
                    // Do nothing while walking
                }
                _busy = false; // Change busy state
                
            }
        }

        GD.Print($"Motivation: {_motivation}");
    }
}
