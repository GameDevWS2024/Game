using Godot;

namespace Game.scripts.AI;

[GlobalClass]
public partial class VisibleForAi : Node2D
{
    public const string GroupName = "AiVisible";
    [Export] public string NameForAi = "";
    [Export]
    public string DescriptionForAi = "";

    public override void _Ready()
    {
        AddToGroup(GroupName);
    }

    public override string ToString()
    {
        return $"{NameForAi}:{DescriptionForAi} at ({GlobalPosition.X:F0}, {GlobalPosition.Y:F0})";
    }
}