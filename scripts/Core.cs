using System;

using Godot;

public partial class Core : Node2D
{

    public const int PIXELSCALE = 1000;
    public int MaterialCount;
    [Export] public float LightRadiusSmallerCircle { get; private set; } = 1000;
    [Export] public float LightRadiusBiggerCircle { get; private set; } = 1500;
    private static PointLight2D? s_coreLight;

    public override void _Ready()
    {
        // Get scale of PointLight2D
        s_coreLight = GetNode<PointLight2D>("%CoreLight");
        this.Scale = s_coreLight.Scale;
        this.Position = s_coreLight.Position;
        MaterialCount = 0;
    }

    public void SetCorePosition(Vector2 position)
    {
        this.Position = position;
    }

    //Drawing both smaller and bigger circle
    public void DrawLightRadius()
    {
        Vector2 center = this.Position;
        DrawArc(center, LightRadiusSmallerCircle, 0, Mathf.Tau, 64, Colors.Red, 4f);
        DrawArc(center, LightRadiusBiggerCircle, 0, Mathf.Tau, 64, Colors.Blue, 4f);
        GD.Print("Drawing circle...");
    }

    public override void _Draw()
    {
        DrawLightRadius();
    }

    public void IncreaseScale()
    {
        s_coreLight!.Scale *= 1.1f;
    }
}
