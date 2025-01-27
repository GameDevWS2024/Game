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
    [Export] public bool ItemActivationStatus { get; set; } = true;

    Core? _core = null;
    private Area2D? _insideArea;
    Godot.Collections.Array<Node> _entities = null!;
    Ally _nearestAlly = null!;
    Array<Node> entities = null!;

    public override void _Ready()
    {
        _core = GetTree().GetNodesInGroup("Core").Cast<Core>().SingleOrDefault();
        entities = GetTree().GetNodesInGroup("Entities");
        float dist = float.MaxValue;
        foreach (Ally ally in entities)
        {
            if (ally.GlobalPosition.DistanceTo(GlobalPosition) <= dist)
            {
                dist = ally.GlobalPosition.DistanceTo(GlobalPosition);
                _nearestAlly = ally;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Array<Node> entities = GetTree().GetNodesInGroup("Entities");
        bool show = false;
        int smallest = int.MaxValue;
        if (delta % delta * 1000 == 0)
        {
            entities = GetTree().GetNodesInGroup("Entities");
            float dist = float.MaxValue;
            foreach (Ally ally in entities)
            {
                if (ally.GlobalPosition.DistanceTo(GlobalPosition) <= dist)
                {
                    dist = ally.GlobalPosition.DistanceTo(GlobalPosition);
                    _nearestAlly = ally;
                }
            }
        }
        foreach (Node entity in entities)
        {
            if (entity is CharacterBody2D body)
            {
                // GD.Print(body!.GlobalPosition);
                if (body != null && body.GlobalPosition.DistanceTo(GlobalPosition) < Radius
                    && (NeedsToBeInInventoryName == Game.Scripts.Items.Material.None || (_nearestAlly.SsInventory.ContainsMaterial(NeedsToBeInInventoryName) && ItemActivationStatus)))
                {
                    smallest = (int)Math.Min(body.GlobalPosition.DistanceTo(GlobalPosition), smallest);
                    show = true;
                }
            }
        }
        if (this.GetName() != "Sprite2D")
        {
            Sprite2D? sprite = GetParent() as Sprite2D;
            SetShowSceneState(sprite, show);
        }
    }

    public static void SetShowSceneState(Sprite2D? sprite, bool state)
    {
        if (sprite != null)
        {
            sprite!.Visible = state;
        }
    }
}