using Godot;

public partial class Removeable : Node
{
    public void Remove()
    {
        GetParent().QueueFree();
    }
}
