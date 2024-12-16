using Game.Scripts;

using Godot;

using System;
using System.Linq;

public partial class SimpleInventoryDisplay : RichTextLabel
{
	// Called when the node enters the scene tree for the first time.
	private Inventory? _inventory;
	public override void _Ready()
	{
		_inventory = GetParent<Core>().Inventory;
		if (_inventory != null)
		{
			_inventory.Changed += UpdateDisplay;
		}
	}

	private void UpdateDisplay()
	{
		if (_inventory is null)
		{
			return;
		}

		Text = string.Join("\n", _inventory.Items.Select(item => $"{item.Key.Name}: {item.Value}"));
		GD.Print(Text);
	}

}
