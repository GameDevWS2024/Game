using Game.Scripts.Items;

using Godot;

namespace Game.Scripts;

public partial class Core : Node2D
{

    public const int PIXELSCALE = 600;
    public static int MaterialCount;
    private static PointLight2D? s_coreLight;
    public static float LightRadiusSmallerCircle { get; private set; }
    public static float LightRadiusBiggerCircle { get; private set; }
    public Inventory? Inventory;

    public override void _Ready()
    {
        Inventory = new Inventory(34);
        Inventory.AddItem(new Itemstack(Game.Scripts.Items.Material.Stone));

        // Get scale of PointLight2D
        s_coreLight = GetNode<PointLight2D>("%CoreLight");
        this.Scale = s_coreLight.Scale;
        this.Position = s_coreLight.Position;
        MaterialCount = 0;
        LightRadiusSmallerCircle = s_coreLight.Scale.X * PIXELSCALE;
        LightRadiusBiggerCircle = s_coreLight.Scale.X * PIXELSCALE * 1.5f;
    }

    private void SetCorePosition(Vector2 position)
    {
        this.Position = position;
    }

    //Drawing both smaller and bigger circle
    private void DrawLightRadius()
    {
        Vector2 center = this.Position;
        DrawArc(center, Core.LightRadiusSmallerCircle, 0, Mathf.Tau, 64, Colors.Red, 4f);
        DrawArc(center, Core.LightRadiusBiggerCircle, 0, Mathf.Tau, 64, Colors.Blue, 4f);
        // GD.Print("Drawing circle...");
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
