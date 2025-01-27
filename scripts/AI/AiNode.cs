using System;

using Godot;
using Game.Scripts.AI;

using Material = Game.Scripts.Items.Material;
public partial class AiNode : Node2D
{
    [Export] private bool _isVisibleForAi = true;
    [Export] private string _nameForAi = "";
    [Export] private string _descriptionForAi = "";

    [Export] private bool _isAddItemOnInteract = true;
    [Export] private Material _fromChosenMaterial;
    [Export] private int _amount = 1;

    [Export] private bool _isShowWhileInRadius;
    [Export] private PackedScene _showWhileInRadiusScene = null!;
    [Export] private int _radius = 100;
    [Export] private Material _needsToBeInInventory;

    [Export] private bool _isRemovable = true;

    [Export] private bool _isInteractable = true;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        VisibleForAI visibleForAi = GetNode<VisibleForAI>("VisibleForAI");
        ItemAdder adder = GetNode<ItemAdder>("ItemAdder");
        ShowWhileInRadius showWhileInRadius = GetNode<ShowWhileInRadius>("ShowWhileInRadius");
        Removeable removable = GetNode<Removeable>("Removeable");
        Interactable interactable = GetNode<Interactable>("Interactable");

        if (_isVisibleForAi)
        {
            visibleForAi.NameForAi = _nameForAi;
            visibleForAi.DescribtionForAi = _descriptionForAi;
        }
        else
        {
            visibleForAi.QueueFree();
        }

        if (!_isInteractable)
        {
            interactable.QueueFree();
        }

        if (!_isRemovable)
        {
            removable.QueueFree();
        }

        if (_isShowWhileInRadius)
        {
            showWhileInRadius.Radius = _radius;
            showWhileInRadius.NeedsToBeInInventoryName = _needsToBeInInventory;
            showWhileInRadius.SceneToShow = _showWhileInRadiusScene;
        }
        else
        {
            showWhileInRadius.QueueFree();
        }

        if (_isAddItemOnInteract)
        {
            adder.Amount = _amount;
            adder.ItemToAdd = _fromChosenMaterial;
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
