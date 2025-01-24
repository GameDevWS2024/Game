using System;

using Godot;

public partial class CameraController : Node
{
    public static Camera2D Camera1 = null!;
    public static Camera2D Camera2 = null!;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Camera1 = GetNode<Camera2D>("Ally1/Ally1Cam");
        Camera2 = GetNode<Camera2D>("Ally2/Ally2Cam");

        Camera1.MakeCurrent();

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
