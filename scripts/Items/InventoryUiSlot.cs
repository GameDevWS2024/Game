using Godot;
using System;

using Game.Scripts.Items;

public partial class InventoryUiSlot : Panel
{
	private Sprite2D _icon = null!;
	public override void _Ready()
	{
		_icon = GetChild<CenterContainer>(1).GetChild<Panel>(0).GetChild<Sprite2D>(0);
		if (_icon == null)
		{
			GD.PrintErr("ERR: Can't find icon <InventoryUISlot.cs>");
		}
	}


	public override void _Process(double delta)
	{
	}

	public void Update(Itemstack item)
	{
		GD.Print("Update slot "+item.Material);
		switch (item.Material)
		{
			case Game.Scripts.Items.Material.None:
				_icon.Visible = false;
				break;
			case Game.Scripts.Items.Material.Diamond:
				_icon.Visible = true;
				_icon.Texture = GD.Load<Texture2D>("res://assets/items/diamond.png");
				break;
			case Game.Scripts.Items.Material.Iron:
				_icon.Visible = true;
				_icon.Texture = GD.Load<Texture2D>("res://assets/items/iron.png");
				break;
			case Game.Scripts.Items.Material.Copper:
				_icon.Visible = true;
				_icon.Texture = GD.Load<Texture2D>("res://assets/items/copper.png");
				break;

			case Game.Scripts.Items.Material.Wood:
				_icon.Visible = true;
				_icon.Texture = GD.Load<Texture2D>("res://assets/items/wood.png");
				break;

			case Game.Scripts.Items.Material.Stone:
				_icon.Visible = true;
				_icon.Texture = GD.Load<Texture2D>("res://assets/items/stone.png");
				break;

			case Game.Scripts.Items.Material.Gold:
				_icon.Visible = true;
				_icon.Texture = GD.Load<Texture2D>("res://assets/items/gold.png");
				break;

			case Game.Scripts.Items.Material.Flashlight:
				_icon.Visible = true;
				_icon.Texture = GD.Load<Texture2D>("res://assets/items/flashlight.png");
				break;

			case Game.Scripts.Items.Material.Notebook:
				_icon.Visible = true;
				_icon.Texture = GD.Load<Texture2D>("res://assets/items/notebook.png");
				break;

		}
	}
}
