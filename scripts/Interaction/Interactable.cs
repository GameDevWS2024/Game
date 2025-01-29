using Godot;

[GlobalClass]
public partial class Interactable : Node2D
{
    public const string GroupName = "Interactable";
    [Signal] public delegate void InteractFromNodeEventHandler(Node caller);
    [Signal] public delegate void InteractEventHandler();

    public bool IsSendSystemMessage = false;
    public string SystemMessageInstructionForAlly = "";

    public override void _Ready()
    {
        AddToGroup(GroupName);
    }

    public void Trigger(Node caller)
    {
        if (IsSendSystemMessage && SystemMessageInstructionForAlly != "")
        {
            GD.Print("caller is: " + caller.Name);
        }
        EmitSignal(SignalName.Interact);
        EmitSignal(SignalName.InteractFromNode, caller);
    }
}
