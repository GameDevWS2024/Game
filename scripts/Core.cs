using Game.Scripts.Items;

using Godot;

namespace Game.Scripts;

public partial class Core : Node2D
{
    private const int Pixelscale = 600;
    private static PointLight2D? s_coreLight;
    public static float LightRadiusSmallerCircle { get; private set; }
    public static float LightRadiusBiggerCircle { get; private set; }
    public Inventory? Inventory = new Inventory(34);

    public override void _Ready()
    {
        // Get scale of PointLight2D
        s_coreLight = GetNode<PointLight2D>("%CoreLight");
        this.Scale = s_coreLight.Scale;
        this.Position = s_coreLight.Position;
        LightRadiusSmallerCircle = s_coreLight.Scale.X * Pixelscale;
        LightRadiusBiggerCircle = s_coreLight.Scale.X * Pixelscale * 1.5f;
    }



    //Drawing both smaller and bigger circle
    private void DrawLightRadius()
    {
        Vector2 center = new Vector2(500, 2000);
        s_coreLight!.SetPosition(center);
        //uncomment to draw circles around core
        //DrawArc(center, Core.LightRadiusSmallerCircle, 0, Mathf.Tau, 64, Colors.Red, 4f);
        //DrawArc(center, Core.LightRadiusBiggerCircle, 0, Mathf.Tau, 64, Colors.Blue, 4f);

    }

    public override void _Draw()
    {
        DrawLightRadius();
    }

    public static void IncreaseScale()
    {
        s_coreLight!.Scale *= 1.1f;
        LightRadiusSmallerCircle *= 1.1f;
        LightRadiusBiggerCircle *= 1.1f;
    }

}
