using System;

using Godot;

[GlobalClass]
public partial class VisibleForAI : Node2D
{
    public const string GroupName = "AiVisible";
    [Export] public string NameForAi = "";
    [Export] public string DescribtionForAi = "";

    [Export] public int VisionRadius = 500;

    public override void _Ready()
    {
        AddToGroup(GroupName);
    }

    public override string ToString()
    {
        return $"{NameForAi} at ({GlobalPosition.X.ToString("F0")}, {GlobalPosition.Y.ToString("F0")}) | {DescribtionForAi}";
    }
}
