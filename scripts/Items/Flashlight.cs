using Godot;
using System;
using Game.Scripts.Items;
using Godot.Collections;
using System.Linq;
using System.Collections.Generic;

public partial class Flashlight : Node2D
{
	private Ally ally = null!;
	private Node _parent = null!;
	private float _radius = 100.0f;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//base._Ready();
		ally = GetTree().CurrentScene.GetNode<Ally>("Ally");
		Interactable interactable = GetNode<Interactable>("Interactible");
		if (interactable != null)
            {
				GD.Print("DEBUG: Interactible found");
				_parent = interactable;
                interactable.Connect("InteractFromNodeEventHandler", new Callable(this, nameof(OnInteract)));
            }
			else 
			{
				GD.Print("DEBUG: no interactable found");
			}
	}

	public void OnInteract(){
		GD.Print("DEBUG: Interaction");
		if(_parent is Ally ally) {
			Inventory inv = ally.GetInventory();
			GD.Print("DEBUG: pulled allies inventory");
			
			if (!inv.HasSpace()) {
				GD.Print("DEBUG: ally has no space");
				return;
			}

			inv.AddItem(new Itemstack(Game.Scripts.Items.Material.Flashlight, false));
			inv.Print();

		ally.GetNode<PointLight2D>("PointLight2D").Enabled = true;
		}
		else{
			GD.Print("DEBUG: Not an ally "+ _parent.GetClass().ToString());
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		float distance = this.GlobalPosition.DistanceTo(ally.GlobalPosition);
		if(distance < _radius){
			Inventory inv = ally.GetInventory();
			if(inv.HasSpace() && !inv.ContainsMaterial(Game.Scripts.Items.Material.Flashlight)) {
				inv.AddItem(new Itemstack(Game.Scripts.Items.Material.Flashlight, false));
				ally.GetNode<PointLight2D>("Flashlight").Enabled = true;
				GD.Print("DEBUG: FlASHLIGHT FOUND");
				inv.Print();
			}
			else
			{
				//GD.Print("DEBUG: no space in inv or has flashlight");
			}
		}
	}
}
