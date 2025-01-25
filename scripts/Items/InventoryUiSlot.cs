using Godot;
using System;

using Game.Scripts.Items;

public partial class InventoryUiSlot : Panel
{
	private Sprite2D _icon = null!;
	public override void _Ready()
	{
		_icon = GetChild<CenterContainer>(1, false).GetChild<Panel>(0, false).GetChild<Sprite2D>(0, false);
	}


	public override void _Process(double delta)
	{
	}

	public void Update(Itemstack item)
	{
		switch (item.Material)
		{
			case Game.Scripts.Items.Material.None:
				_icon.Visible = false;
				break;
			case Game.Scripts.Items.Material.Diamond:
				_icon.Visible = true;
				_icon.Texture = GD.Load<Texture2D>("res://assets/items/diamond.png");
				break;
		}
	}
}
