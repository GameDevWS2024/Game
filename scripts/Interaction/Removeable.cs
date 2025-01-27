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
        GetParent().CallDeferred(Node.MethodName.QueueFree);
    }
}
