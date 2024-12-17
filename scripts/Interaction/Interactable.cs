using System.Collections.Generic;
using System.Linq;

using Godot;

[GlobalClass]
public partial class Interactable : Node2D
{
    public const string GroupName = "Interactable";
    [Signal] public delegate void InteractedEventHandler();
    [Signal] public delegate void NodeInteractedEventHandler(Node caller);
    [Signal] public delegate void FailedToInteractEventHandler();
    [Signal] public delegate void NodeFailedToInteractEventHandler(Node caller);

    public override void _Ready()
    {
        AddToGroup(GroupName);
    }

    public void Interact(Node caller)
    {
        bool interactionSucessfull = GetChildren().OfType<IInteractCondition>().All(condition => condition.CheckCondition(caller));

        if (interactionSucessfull)
        {
            EmitSignal(SignalName.Interacted);
            EmitSignal(SignalName.NodeInteracted, caller);
        }
        else
        {
            EmitSignal(SignalName.FailedToInteract);
            EmitSignal(SignalName.NodeFailedToInteract, caller);
        }

    }
}
