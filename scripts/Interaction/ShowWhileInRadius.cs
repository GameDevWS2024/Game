using System;
using System.Collections.Generic;
using System.Linq;

using Game.scripts.Interaction;
using Game.Scripts;
using Game.Scripts.AI;

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
    private bool debugOnce = false;

    Core? _core = null;
    private Area2D? _insideArea;
    Godot.Collections.Array<Node> _entities = null!;
    Ally _nearestAlly = null!;
    Array<Node> entities = null!;

    // Load the scene you want to instance.   ONLY FOR CHEST INSIDE BIG HOUSE
    private PackedScene _sceneToInstance = null!;

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
        _sceneToInstance = GD.Load<PackedScene>("res://scenes/prefabs/ai_node.tscn");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (ItemActivationStatus == true && !debugOnce)
        {
            GD.Print("ItemActivationStatus = true");
            debugOnce = true;
        }
        base._PhysicsProcess(delta);
        Array<Node> entities = GetTree().GetNodesInGroup("Entities");
        bool show = false;
        int smallest = int.MaxValue;
        if (delta % delta * 2000 == 0)
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
            //  GD.Print("nearestAlly = " + _nearestAlly.GetName());
            // GD.Print("needsToBeInInventoryName: " + NeedsToBeInInventoryName + ", has it: " +
            //          _nearestAlly.SsInventory.ContainsMaterial(NeedsToBeInInventoryName));
        }
        foreach (Node entity in entities)
        {
            if (entity is CharacterBody2D body)
            {
                // GD.Print(body!.GlobalPosition);
                /*GD.Print((body.GlobalPosition.DistanceTo(GlobalPosition) < Radius) + " dist cond., "+(NeedsToBeInInventoryName == Game.Scripts.Items.Material.None || (_nearestAlly.SsInventory.ContainsMaterial(NeedsToBeInInventoryName) && ItemActivationStatus)));
                GD.Print(show + " show state");
                GD.Print(_nearestAlly.SsInventory.ContainsMaterial(NeedsToBeInInventoryName));
                GD.Print(ItemActivationStatus);
                GD.Print(GetParent().GetParent().GetName()+"\n"); */
                if (body.GlobalPosition.DistanceTo(GlobalPosition) < Radius
                         && (NeedsToBeInInventoryName == Game.Scripts.Items.Material.None || (_nearestAlly.SsInventory.ContainsMaterial(NeedsToBeInInventoryName) && _nearestAlly.lit)))
                {
                    show = true;

                    if (this.GetName() != "Sprite2D")
                    {
                        Sprite2D? sprite = GetParent() as Sprite2D;
                        if (sprite != null)
                        {
                            sprite!.Visible = true;
                        }
                    }
                    else
                    {
                        GD.Print("this is Sprite2D error");
                    }

                    if (this.GetName() == "ChestInsideHouse")
                    {
                        SpawnChild();
                        // has AiNode (Node2D) now
                        AiNode aiNode = GetNode<AiNode>("AiNode");
                        aiNode.IsVisibleForAi = true;
                        aiNode.NameForAi = "Wooden Chest";
                        aiNode.DescriptionForAi = "Seems to be locked. Maybe there's a key in the city?";
                        aiNode.IsInteractable = true;
                        aiNode.IsAddItemOnInteract = true;
                        aiNode.Amount = 1;
                        aiNode.FromChosenMaterial = Game.Scripts.Items.Material.FestiveStaff;
                    }

                }
            }
        }


    }

    private void SpawnChild()
    {
        if (_sceneToInstance != null)
        {
            // Instance the scene.
            Node2D instance = _sceneToInstance.Instantiate<Node2D>();

            // Add the instance as a child of the current node.
            AddChild(instance);
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
