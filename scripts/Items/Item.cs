using System;

using Godot;

[GlobalClass]
public partial class Item : Resource
{
    [Export] public string Name { get; set; } = "";
    [Export] public string Description { get; set; } = "";
    [Export] public Texture2D? Icon { get; set; }

    public bool Equals(Item other)
    {
        if (other is null)
        {
            return false;
        }

        return Name == other.Name && Description == other.Description;
    }

    public override bool Equals(object? obj)
    {
        return obj is Item other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Description);
    }
}
