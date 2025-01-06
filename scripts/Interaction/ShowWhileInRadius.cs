/*
using Game.Scripts;

using Godot;

using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class ShowWhileInRadius : Node
{
    [Export] public PackedScene? SceneToShow { get; set; }
    [Export] public int Radius { get; set; }
    [Export] public Item NeedsToBeInInventory { get; set; } = new Item();

    public override void _Ready()
    {
        Core? core = GetTree().GetNodesInGroup(Core.GroupName).OfType<Core>().FirstOrDefault();
        List<Entity> Entities = GetTree().GetNodesInGroup("Entities");
        // get distance from every entity to this's global position
        // if some entity has distance < Radius and inventory contains NeedsToBeInInventory
        if (true)  // (core!.Inventory.containsItem(NeedsToBeInInventory))
        {
            Interactable interactable = GetParent().GetNode<Interactable>("Interactable");
            if (interactable != null)
            {
                interactable.Interact += ShowScene;
            }
        }
        else
        {
            HideScene();
        }
    }

    // Make this public so it can be called from anywhere
    public void ShowScene()
    {
        SceneToShow!.Instantiate();
    }

    public void HideScene()
    {
        GetParent().FindChild(SceneToShow!.GetName()).Dispose();
    }
}
*/