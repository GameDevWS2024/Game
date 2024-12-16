using Game.Scripts;

using Godot;

using System;
using System.Linq;

[GlobalClass]
public partial class ItemAdder : InteractionListener
{
	[Export] public Item? ItemToAdd { get; set; }
	[Export] public int Amount { get; set; } = 1;


	public void AddItem()
	{
		Core? core = GetTree().GetNodesInGroup(Core.GroupName).OfType<Core>().FirstOrDefault();

		if (core != null && ItemToAdd != null)
		{
			core.Inventory.AddItem(ItemToAdd, Amount);
		}
	}

	public override void OnInteract(Node caller)
	{
		AddItem();
	}
}
