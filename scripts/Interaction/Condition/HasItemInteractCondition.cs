using Godot;
using Game.Scripts;
using System.Linq;

[GlobalClass]
public partial class HasItemInteractCondition : Node, IInteractCondition
{

	[Export] public Item? ItemToHave { get; set; }
	[Export] public int Amount { get; set; } = 1;

	[Export] public bool Not;
	public bool CheckCondition(Node caller)
	{
		return HasItem() ^ Not;
	}

	private bool HasItem()
	{
		if (ItemToHave == null)
		{
			return true;
		}

		Core? core = GetTree().GetNodesInGroup(Core.GroupName).OfType<Core>().FirstOrDefault();

		return core is not null && core.Inventory.GetItemQuantity(ItemToHave) >= Amount;
	}
}
