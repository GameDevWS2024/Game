using Godot;
using System;

public partial class InventoryUi : Control
{
 
	private bool _isOpen = false;
	
	public override void _Ready()
	{
		Visible = false;
	}
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("e"))
		{
			if (_isOpen)
			{
				Close();
			}
			else
			{
				Open();
			}
		}
	}

	private void Open()
	{
		_isOpen = true;
		Visible = true;
	}

	private void Close()
	{
		_isOpen = false;
		Visible = false;
	}
}
