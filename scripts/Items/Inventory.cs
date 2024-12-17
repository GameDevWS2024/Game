using System;
using System.Collections.Generic;
using System.Linq;

using Godot;

public partial class Inventory : Resource
{
    [Signal] public delegate void ChangedEventHandler();
    private readonly Dictionary<Item, int> _items = [];
    public IReadOnlyDictionary<Item, int> Items => _items;

    public void AddItem(Item item, int amount = 1)
    {
        if (item is null || amount <= 0)
        {
            return;
        }

        if (_items.ContainsKey(item))
        {
            _items[item] += amount;
        }
        else
        {
            _items[item] = amount;
        }

        EmitSignal(SignalName.Changed);
    }

    public bool TryRemoveItem(Item item, int amount = 1)
    {
        if (item is null || amount <= 0)
        {
            return false;
        }

        if (_items.ContainsKey(item) && _items[item] >= amount)
        {
            _items[item] -= amount;

            if (_items[item] == 0)
            {
                _items.Remove(item);
            }

            EmitSignal(SignalName.Changed);
            return true;
        }

        return false;
    }

    public int GetItemQuantity(Item item)
    {
        return _items.GetValueOrDefault(item, 0);
    }

    public int GetTotalItemCount()
    {
        return _items.Values.Sum();
    }

}
