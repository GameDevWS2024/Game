using System;
using System.Linq;

using Game.Scripts;
using Game.Scripts.Items;

using Godot;
using Godot.Collections;

public partial class InventoryUi : Control
{

    private bool _isOpen = false;
    private Array<InventoryUiSlot> _slots = null!;
    private Ally _parent = null!;

    public override void _Ready()
    {
        _parent = GetParent<Ally>();
        _slots = [];

        Visible = false;
        foreach (Panel child in GetChild(0).GetChild(0).GetChildren().OfType<Panel>())
        {
            _slots.Add(child as InventoryUiSlot);
        }
    }
    public override void _Process(double delta)
    {
        if (_isOpen)
        {
            UpdateSlots();
        }

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

    private void UpdateSlots()
    {
        Inventory inv = _parent.Inventory;
        for (int i = 0; i < Math.Min(_slots.Count, inv.Size); i++)
        {
            _slots[i].Update(inv.GetItem(i));
        }
    }

}
