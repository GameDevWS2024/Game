using System;
using System.Collections.Generic;

using Godot;
namespace Game.Scripts.Items;
public class Inventory
{
    private int Size { get; }
    private Itemstack[] Items { get; set; }



    public Inventory(int size)
    {
        Size = size;
        Items = new Itemstack[size];
        for (int i = 0; i < size; i++)
        {
            Items[i] = new Itemstack(Material.None);
        }

        AddItem(new Itemstack(Material.Wood, 27));
        AddItem(new Itemstack(Material.Wood, 64));

        Print();
    }

    override
    public string ToString()
    {
        string inv = "Inventory: [";

        for (int i = 0; i < Size; i++)
        {
            inv += Items[i].Material + " : " + Items[i].Amount + ", ";
            if (Items[i]!.Material != Material.None)
            {
                bool isEmpty = false;
                inv += Items[i]!.Material + " : " + Items[i]!.Amount + ", ";
            }
        }

        if (inv.EndsWith(", "))
        {
            inv = inv.Remove(inv.Length - 2);
        }

        inv += "]";

        return inv;
    }

    public void Print()
    {
        GD.Print(ToString());
    }

    public bool FitItem(Itemstack stack)
    {
        Material mat = stack.Material;
        for (int i = 0; i < Size; i++)
        {
            if (Items[i] is { Material: Material.None }) { return true; }
            if (Items[i]!.Material == mat && Items[i]!.Amount < 64) { return true; }
        }
        return false;
    }

    public bool HasSpace()
    {
        foreach (Itemstack item in Items)
        {
            if (item.Material == Material.None)
            {
                return true;
            }
        }

        return false;
    }

    public void AddItem(Itemstack itemstack)
    {
        if (itemstack.Amount == 0) { return; }

        int none = -1;
        for (int i = 0; i < Size; i++)
        {

            if (Items[i]!.Material == itemstack.Material && Items[i]!.Stackable && Items[i]!.Amount < 64)
            {
                int space = 64 - Items[i]!.Amount;
                if (itemstack.Amount <= space)
                {
                    Items[i]!.Amount += itemstack.Amount;
                    itemstack.Amount = 0;
                }
                else
                {
                    Items[i]!.Amount += space;
                    itemstack.Amount -= space;
                    AddItem(itemstack); // recursively fill the inventory until stack is empty or inventory is full.
                }

                return;
            }

            if (Items[i]!.Material == Material.None && none == -1)
            {
                none = i;
            }
        }
        if (none != -1)
        {
            if (itemstack.Amount <= 64)
            {
                Items[none] = new Itemstack(itemstack.Material, itemstack.Amount, itemstack.Stackable);
                itemstack.Amount = 0;
            }
            else
            {
                Items[none] = new Itemstack(itemstack.Material, 64, itemstack.Stackable);
                itemstack.Amount -= 64;
                AddItem(itemstack); // recursively fill the inventory until stack is empty or inventory is full.
            }
        }
    }

    public void SetItem(Itemstack itemstack, int i)
    {
        if (i < 0 || i >= Size)
        {
            throw new Exception("Given index is out of bounds for inventory size " + Size);
        }

        Items[i] = itemstack;
    }

    public Itemstack ExtractItem(int i)
    {
        if (i < 0 || i >= Size)
        {
            throw new Exception("Given index is out of bounds for inventory size " + Size);
        }
        Itemstack rtrn = Items[i];
        Items[i] = new Itemstack(Material.None);
        return rtrn;
    }

    public void DeleteItem(int i)
    {
        if (i < 0 || i >= Size)
        {
            throw new Exception("Given index is out of bounds for inventory size " + Size);
        }
    }

    public void SwapItems(int i, int j)
    {
        if (i < 0 || i >= Size || j < 0 || j >= Size)
        {
            throw new Exception("Given index is out of bounds for inventory size " + Size);
        }

        Itemstack temp = Items[i];
        Items[i] = Items[j];
        Items[j] = Items[i];
    }

    public void Clear()
    {
        for (int i = 0; i < Size; i++)
        {
            Items[i] = new Itemstack(Material.None);
        }
    }

    public Itemstack[] GetItems()
    {
        return Items;
    }
}
