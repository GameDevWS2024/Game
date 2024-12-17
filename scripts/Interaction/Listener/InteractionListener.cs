using Godot;
public abstract partial class InteractionListener : Node
{

    [Export] public bool ListenToInteracted { get; set; } = true;
    [Export] public bool ListenToFailedToInteract { get; set; } = false;

    public override void _Ready()
    {
        if (!ListenToInteracted && !ListenToFailedToInteract)
        {
            return;
        }

        Interactable interactable = GetParent().GetNode<Interactable>("Interactable");
        if (interactable == null)
        {
            return;
        }

        if (ListenToInteracted)
        {
            interactable.NodeInteracted += OnInteract;
        }
        if (ListenToFailedToInteract)
        {
            interactable.NodeFailedToInteract += OnInteract;
        }

    }

    public abstract void OnInteract(Node caller);
}
