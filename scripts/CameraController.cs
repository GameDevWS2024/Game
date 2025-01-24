using System;

using Godot;

public partial class CameraController : Node
{
    public static Camera2D _camera1;
    public static Camera2D _camera2;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _camera1 = GetNode<Camera2D>("Ally1/Ally1Cam");
        _camera2 = GetNode<Camera2D>("Ally2/Ally2Cam");

        _camera1.MakeCurrent();

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
