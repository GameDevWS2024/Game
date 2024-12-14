using Godot;

public partial class Interactable : Node2D
{
    public const string GroupName = "Interactable";
    [Signal] public delegate void InteractFromNodeEventHandler(Node caller);
    [Signal] public delegate void InteractEventHandler();

    public override void _Ready()
    {
        AddToGroup(GroupName);
    }

    public void Trigger(Node caller)
    {
        EmitSignal(SignalName.Interact);
        EmitSignal(SignalName.InteractFromNode, caller);
    }
}
