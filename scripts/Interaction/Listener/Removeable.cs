using Godot;

[GlobalClass]
public partial class Removeable : InteractionListener
{
    public void Remove()
    {
        GetParent().CallDeferred(Node.MethodName.QueueFree);
    }
    public override void OnInteract(Node caller)
    {
        Remove();
    }

}
