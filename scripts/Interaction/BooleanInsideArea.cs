using Godot;
using System;

public partial class BooleanInsideArea : Area2D
{
    [Export(PropertyHint.None, "Welcher Node hat das Skript, das wissen muss, ob die Fläche verlassen wurde?")] public NodePath? TargetNodePath; // Hier den Pfad zum anderen Knoten angeben
    public bool BodyInArea { get; set; } = false;
    
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
        GD.Print("ShowWhileInRadius _PhysicsProcess: IsInArea = ", BodyInArea);
	}
    public void OnBodyEntered(Node body)
    {
        BodyInArea = true;
        UpdateTargetNode();
    }

    public void OnBodyExited(Node body)
    {
        BodyInArea = false;
        UpdateTargetNode();
    }
    
    private void UpdateTargetNode()
    {
        GD.Print("UpdateTargetNode called");
        GD.Print("TargetNodePath: ", TargetNodePath);
        GD.Print("BodyInArea: ", BodyInArea);
        if (TargetNodePath != null)
        {
            Node? targetNode = GetNode<Node>(TargetNodePath);
            ShowWhileInRadius showWhileInRadius = targetNode.GetNode<ShowWhileInRadius>("ShowWhileInRadius");
            showWhileInRadius.IsInArea = BodyInArea;
        }
        else
        {
            GD.PrintErr("No target node found");
        }
    }
    
}
