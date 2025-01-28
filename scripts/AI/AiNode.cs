using System;

using Game.Scripts.AI;

using Godot;

using Material = Game.Scripts.Items.Material;
public partial class AiNode : Node2D
{
    [Export] public bool IsVisibleForAi = true;
    [Export] public string NameForAi = "";
    [Export] public string DescriptionForAi = "";

    [Export] public bool IsAddItemOnInteract = true;
    [Export] public Material FromChosenMaterial;
    [Export] public int Amount = 1;

    [Export] public bool IsShowWhileInRadius;
    [Export] public PackedScene ShowWhileInRadiusScene = null!;
    [Export] public int Radius = 100;
    [Export] public Material NeedsToBeInInventory;

    [Export] public bool IsRemovable = true;

    [Export] public bool IsInteractable = true;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        VisibleForAI visibleForAi = GetNode<VisibleForAI>("VisibleForAI");
        Game.Scripts.Interaction.ItemAdder adder = GetNode<Game.Scripts.Interaction.ItemAdder>("ItemAdder");
        ShowWhileInRadius showWhileInRadius = GetNode<ShowWhileInRadius>("ShowWhileInRadius");
        Removeable removable = GetNode<Removeable>("Removeable");
        Interactable interactable = GetNode<Interactable>("Interactable");

        if (IsVisibleForAi)
        {
            visibleForAi.NameForAi = NameForAi;
            visibleForAi.DescribtionForAi = DescriptionForAi;
        }
        else
        {
            visibleForAi.QueueFree();
        }

        if (!IsInteractable)
        {
            interactable.QueueFree();
        }

        if (!IsRemovable)
        {
            removable.QueueFree();
        }

        if (IsShowWhileInRadius)
        {
            showWhileInRadius.Radius = Radius;
            showWhileInRadius.NeedsToBeInInventoryName = NeedsToBeInInventory;
            showWhileInRadius.SceneToShow = ShowWhileInRadiusScene;
        }
        else
        {
            showWhileInRadius.QueueFree();
        }

        if (IsAddItemOnInteract)
        {
            adder.Amount = Amount;
            adder.ItemToAdd = FromChosenMaterial;
        }
        else
        {
            adder.QueueFree();
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
