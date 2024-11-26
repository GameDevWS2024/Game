using System;
namespace Game.Scripts.Items;
public class Itemstack
{
    public Material Material {get; private set;}
    public readonly bool Stackable;
    private int _amount;
    public int Amount
    {
        get => _amount;
        set
        {
            _amount = value is < 0 or > 64
                ? throw new ArgumentException("Amount cannot be negative and must be smaller than 64")
                : value;
        }
    }


    public Itemstack(Material material) {
        Material = material;
        Amount = 1;
        Stackable = material != Material.None && Stackable;
    }

    public Itemstack(Material material, int amount) 
    {
        Material = material;
        Amount = amount;
        Stackable = material == Material.None ? false : Stackable;
    }

    public Itemstack(Material material, bool stackable) {
        Material = material;
        Amount = 1;
        Stackable = material == Material.None ? false : stackable;
    }

    public Itemstack(Material material, int amount, bool stackable) 
    {
        Material = material;
        Amount = amount;
        Stackable = material == Material.None ? false : stackable;
    }
}