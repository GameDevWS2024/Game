using System.Collections.Generic;

using Game.Scripts;
using Game.Scripts.AI;

using Godot;

[GlobalClass]
public partial class Removeable : Node
{
    [Export] public bool ListenToInteract { get; set; } = true;
    public override void _Ready()
    {
        if (ListenToInteract)
        {
            Interactable interactable = GetParent().GetNode<Interactable>("Interactable");
            if (interactable != null)
            {
                interactable.Interact += Remove;
            }
        }
    }
    public void Remove()
    {
        GD.Print("Removed something");
        VisibleForAI visibleForAI = GetParent().GetNode<VisibleForAI>("VisibleForAI");
        Godot.Collections.Array<Node> entityGroup = GetTree().GetNodesInGroup("Entities");
        Chat? chat = null!;
        foreach (Ally entity in entityGroup)
        {
            if (entity.Name.ToString().Contains('2')){
                chat = entity.GetNode<Chat>("Ally2Cam/Ally2Chat");
            }
            else {
                chat = entity.GetNode<Chat>("Ally1Cam/Ally1Chat");
            }
            chat.AlreadySeen.Remove(visibleForAI);
        }
        GetParent().CallDeferred(Node.MethodName.QueueFree);
    }
}
