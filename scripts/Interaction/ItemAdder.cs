using System;
using System.Linq;

using Game.Scripts;
using Game.Scripts.Items;

using Godot;

using Material = Game.Scripts.Items.Material;

[GlobalClass]
public partial class ItemAdder : Node
{
    [Export] public String? ItemToAddName { get; set; }
    public Material ItemToAdd { get; set; }
    [Export] public int Amount { get; set; }
    [Export] public bool ListenToInteract { get; set; } = true;

    public override void _Ready()
    {
        if (Enum.TryParse(ItemToAddName, out Material parsedMaterial))
        {
            ItemToAdd = parsedMaterial;
            GD.Print("item: " + ItemToAddName);
        }
        else
        {
            GD.Print("item not found");
        }
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
        GD.Print("item: " + ItemToAddName);
        Core? core = GetTree().GetNodesInGroup("Core").OfType<Core>().FirstOrDefault();

        if (core != null)
        {
            core!.Inventory!.AddItem(new Itemstack(ItemToAdd));
            core.Inventory.Print();
        } 
    }
}
