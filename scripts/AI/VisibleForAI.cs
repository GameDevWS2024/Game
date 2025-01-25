using System;

using Godot;

[GlobalClass]
public partial class VisibleForAI : Node2D
{
    public const string GroupName = "AiVisible";
    [Export] public string NameForAi = "";
    [Export] public string DescribtionForAi = "";

    public override void _Ready()
    {
        AddToGroup(GroupName);
    }

    public override string ToString()
    {
        return $"{NameForAi}:{DescribtionForAi} at ({GlobalPosition.X.ToString("F0")}, {GlobalPosition.Y.ToString("F0")})";
    }
}
