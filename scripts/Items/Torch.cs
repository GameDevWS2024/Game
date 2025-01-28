using System;
using System.Collections.Generic;
using System.Linq;

using Game.Scripts;

using Godot;

public partial class Torch : Node
{
    Godot.Collections.Array<Ally> _allies = null!;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _allies = new Godot.Collections.Array<Ally>();

        foreach (Ally ally in GetTree().GetNodesInGroup("Entities"))
        {
            _allies.Add(ally);
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
