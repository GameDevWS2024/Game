
using Game.Scripts;

using Godot;

using System;
using System.Collections.Generic;
using System.Linq;

using Godot.Collections;
using Material = Game.Scripts.Items.Material;

[GlobalClass]
public partial class ShowWhileInRadius : Node
{
    [Export] public PackedScene? SceneToShow { get; set; }
    [Export] public int Radius { get; set; }
    
    public bool IsInArea { get; set; } = false;

    [Export] public Material NeedsToBeInInventoryName { get; set; } = Material.None;
    Core? core = null;
    private Area2D? insideArea;

    public override void _Ready()
    {
        core = GetTree().Root.GetNode<Core>("Core");
    }

    public override void _PhysicsProcess(double delta)
    {
        GD.Print("IsInArea: ", IsInArea);
        
        base._PhysicsProcess(delta);
        Array<Node> entities = GetTree().GetNodesInGroup("Entities");
        bool show = false;
        float smallest = float.MaxValue;
        foreach (Node entity in entities)
        {
            if (entity is CharacterBody2D body)
            {
                // GD.Print(body!.GlobalPosition);
                if (body.GlobalPosition.DistanceTo(GetParent<Node2D>().GlobalPosition) <
                    Radius && (NeedsToBeInInventoryName.Equals(Material.None) ||
                               core!.Inventory!.ContainsMaterial(NeedsToBeInInventoryName)))
                {
                    smallest = Math.Min(body.GlobalPosition.DistanceTo(GetParent<Node2D>().GlobalPosition), smallest);
                    show = true;
                }
            }
        }

        if (show || IsInArea) 
        {
            Interactable interactable = GetParent().GetNode<Interactable>("Interactable");
            if (interactable != null)
            {
                interactable.Interact += ShowScene;
            }
            ShowScene();
        }
        else
        {
            HideScene();
        }
    }
    

// Make this public so it can be called from anywhere
    // Make this public so it can be called from anywhere
    public void ShowScene()
    {
        Sprite2D? parent = GetParent() as Sprite2D;
        parent!.Visible = true;

    }

    public void HideScene()
    {
        Sprite2D? parent = GetParent() as Sprite2D;
        parent!.Visible = false;
    }
}