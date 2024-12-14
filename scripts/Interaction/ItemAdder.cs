using Game.Scripts;

using Godot;

using System;
using System.Linq;

[GlobalClass]
public partial class ItemAdder : Node
{
	[Export] public Item? ItemToAdd { get; set; }
	[Export] public int Amount { get; set; }
	[Export] public bool ListenToInteract { get; set; } = true;

	public override void _Ready()
	{
		if (ListenToInteract)
		{
			Interactable interactable = GetParent().GetNode<Interactable>("Interactable");
			if (interactable != null)
			{
				interactable.Interact += AddItem;
			}
		}
	}

	// Make this public so it can be called from anywhere
	public void AddItem()
	{
		Core? core = GetTree().GetNodesInGroup(Core.GroupName).OfType<Core>().FirstOrDefault();

		GD.Print(core == null);
		GD.Print(ItemToAdd == null);
		if (core != null && ItemToAdd != null)
		{
			GD.Print("wow");
			core.Inventory.AddItem(ItemToAdd, Amount);
		}
	}
}
