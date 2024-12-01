using Godot;
using System;

public partial class Core : Node2D 
{
	
	public const int PIXELSCALE = 1000;
	private float _lightRadiusSmallerCircle;
	private float _lightRadiusBiggerCircle;
	private Vector2 _position;
	private Vector2 _scale;

	public Vector2 Position
	{
		get => _position;
		set => _position = value;
	}

	public Vector2 Scale
	{
		get => _scale;
		set =>	_scale = value;
	}

	public float LightRadius
	{
		get => _lightRadiusSmallerCircle;
		set{
			this._lightRadiusSmallerCircle = value;
			this._lightRadiusBiggerCircle = this._lightRadiusSmallerCircle * 1.5f;
		}
	}

	public override void _Ready()
	{		
		// Get scale of PointLight2D
		PointLight2D coreLight = GetNode<PointLight2D>("%CoreLight");
		this.Scale = coreLight.Scale;
		this.Position = coreLight.Position;
		
		// Calculate lightRadius
		this.LightRadius = this.Scale.X * PIXELSCALE;
		GD.Print(this.LightRadius);
	}

	public void SetCorePosition(Vector2 position)
	{
		this.Position = position;
	}
	
	//Core upscale
	public void ScaleCore(int scale) {
		this.Scale = new Vector2(scale, scale);
		this.LightRadius = scale * PIXELSCALE;
		
	}
	
	//Drawing both smaller and bigger circle
	public void DrawLightRadius(){
		Vector2 center = this._position;
		float radius = _lightRadiusSmallerCircle;
		Color colorRed = new Color(1, 0, 0, 1);
		Color colorBlue = new Color(0, 0, 1, 1);
		DrawArc(center, radius, 0, Mathf.Tau, 64, colorRed, 4f);
		DrawArc(center, radius * 1.5f, 0, Mathf.Tau, 64, colorBlue, 4f);
			GD.Print("Drawing circle...");
	}

	public override void _Draw() {
		DrawLightRadius();
	}
}
