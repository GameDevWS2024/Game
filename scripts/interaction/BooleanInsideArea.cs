using System;

using Godot;

public partial class BooleanInsideArea : Area2D
{
    [Export(PropertyHint.NodePathToEditedNode, "Welcher Node hat das Skript, das wissen muss, ob die Fläche verlassen wurde?")] public NodePath? TargetNodePath; // Hier den Pfad zum anderen Knoten angeben
    public bool BodyInArea { get; set; } = false;

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
            GD.Print("Node not found");
            GD.PrintErr("No target node found");
        }
    }

}
