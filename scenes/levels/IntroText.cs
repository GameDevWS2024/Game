using Godot;

public partial class IntroText : CanvasLayer
{
	private Panel _panel = null!;
	private Label _label = null!;
	private Button _closeButton = null!;

	public override void _Ready()
	{
		
		GD.Print("Ready wird ausgeführt!");
		// Referenzen holen
		_panel = GetNode<Panel>("Panel");
		_label = _panel.GetNode<Label>("Label");
		_closeButton = _panel.GetNode<Button>("Close");

		// Panel zentrieren und Größe anpassen
		_panel.AnchorLeft = 0.5f;
		_panel.AnchorTop = 0.5f;
		_panel.AnchorRight = 0.5f;
		_panel.AnchorBottom = 0.5f;

		_panel.OffsetLeft = -300; // Breite 300
		_panel.OffsetTop = -200; // Höhe 200
		_panel.OffsetRight = 300;
		_panel.OffsetBottom = 200;

		// Label und Button positionieren
		_label.Text = "Stay in the light! Say 'Harvest' to tell your ally to get some resources";
		_label.AnchorLeft = 0.1f;
		_label.AnchorTop = 0.1f;
		_label.AnchorRight = 0.9f;
		_label.AnchorBottom = 0.5f;

		_closeButton.Text = "Weiter";
		_closeButton.AnchorLeft = 0.3f;
		_closeButton.AnchorTop = 0.6f;
		_closeButton.AnchorRight = 0.7f;
		_closeButton.AnchorBottom = 0.8f;

		// Button-Signal verbinden
		_closeButton.Pressed += _on_close_pressed;

		// Spiel pausieren
		GetTree().Paused = true;

		// Panel sichtbar machen
		_panel.Visible = true;
	}

	private void _on_close_pressed()
	{
		
		GD.Print("Button wurde gedrückt!"); // Debug-Ausgabe
		_panel.Visible = false; // Panel verstecken
		GetTree().Paused = false; // Spiel fortsetzen
	}
}
