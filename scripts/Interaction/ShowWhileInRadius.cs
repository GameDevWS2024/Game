
using System;
using System.Collections.Generic;
using System.Linq;

using Game.Scripts;

using Godot;
using Godot.Collections;

using Material = Game.Scripts.Items.Material;

[GlobalClass]
public partial class ShowWhileInRadius : Node2D
{
    [Export] public PackedScene? SceneToShow { get; set; }
    [Export] public int Radius { get; set; }

    public bool IsInArea { get; set; } = false;

    [Export] public Material NeedsToBeInInventoryName { get; set; }
    Core? _core = null;
    private Area2D? _insideArea;

    public override void _Ready()
    {
        _core = GetTree().GetNodesInGroup("Core").Cast<Core>().SingleOrDefault();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Array<Node> entities = GetTree().GetNodesInGroup("Entities");
        bool show = false;
        int smallest = int.MaxValue;
        foreach (Node entity in entities)
        {
            if (entity is CharacterBody2D body)
            {
                // GD.Print(body!.GlobalPosition);
                if (body.GlobalPosition.DistanceTo(GlobalPosition) < Radius
                    && (NeedsToBeInInventoryName == Game.Scripts.Items.Material.None || _core!.Inventory!.ContainsMaterial(NeedsToBeInInventoryName)))
                {
                    smallest = (int)Math.Min(body.GlobalPosition.DistanceTo(GlobalPosition), smallest);
                    show = true;
                }
            }
        }
        if (this.GetName() != "Sprite2D")
        {
            Sprite2D? sprite = GetParent() as Sprite2D;
            if (sprite != null)
            {
                //GD.Print("Sprite2D name " + opa.GetName());
                // sprite is not null!!
            }
            else
            {
                GD.PrintErr("ShowWhileInRadius is null");
            }
            SetShowSceneState(sprite, show);
        }
    }


    // Make this public so it can be called from anywhere
    // Make this public so it can be called from anywhere
    public static void SetShowSceneState(Sprite2D? sprite, bool state)
    {
        if (sprite != null)
        {
            sprite!.Visible = state;
        }
    }
}
