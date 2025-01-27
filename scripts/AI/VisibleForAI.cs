using Godot;

namespace Game.Scripts.AI;

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
        return $"{NameForAi} at ({GlobalPosition.X:F0}, {GlobalPosition.Y:F0}): [Description] {DescribtionForAi}";
    }
}