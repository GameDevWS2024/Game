using System;

using Godot;

public partial class Core : Node2D // Node2D ist passend, da du mit `position` und `scale` arbeitest
{

    public const int _PIXELSCALE = 1000;
    private float _lightRadiusSmallerCircle;
    private float _lightRadiusBiggerCircle;
    private Vector2 _position;
    private Vector2 _scale;

    // Automatische Eigenschaften
    public Vector2 Position
    {
        get => _position;
        set => _position = value; // Aktualisiert auch den tatsächlichen Node2D
    }

    public Vector2 Scale
    {
        get => _scale;
        set => _scale = value;
    }

    public float LightRadius
    {
        get => _lightRadiusSmallerCircle;
        set
        {
            this._lightRadiusSmallerCircle = value;
            this._lightRadiusBiggerCircle = this._lightRadiusSmallerCircle * 1.5f;
        }
    }

    // Initialisierung in _Ready statt im Konstruktor
    public override void _Ready()
    {
        // Scale vom PointLight2D ermitteln
        PointLight2D coreLight = GetNode<PointLight2D>("%CoreLight");
        this.Scale = coreLight.Scale;
        this.Position = coreLight.Position;

        // Licht Radius berechnen mit Core Scale und konstante Pixel Scale die je nach größe der endgültigen Map angepasst werden sollte
        this.LightRadius = this.Scale.X * _PIXELSCALE;
        GD.Print(this.LightRadius);
    }

    // Position für Core ändern 
    public void SetCorePosition(Vector2 position)
    {
        this.Position = position;
    }

    //Core upscalen: Der X und Y Wert wird des Cores wird angepasst und der Licht Radius vergrößert
    public void ScaleCore(int scale)
    {
        this.Scale = new Vector2(scale, scale);
        this.LightRadius = scale * _PIXELSCALE;

    }

    public void _DrawLightRadius()
    {
        Vector2 center = this._position;
        float radius = _lightRadiusSmallerCircle;
        Color colorRed = new Color(1, 0, 0, 1);
        Color colorBlue = new Color(0, 0, 1, 1);
        DrawArc(center, radius, 0, Mathf.Tau, 64, colorRed, 4f);
        DrawArc(center, radius * 1.5f, 0, Mathf.Tau, 64, colorBlue, 4f);
        GD.Print("Drawing circle...");
    }

    public override void _Draw()
    {
        _DrawLightRadius();
    }
}
