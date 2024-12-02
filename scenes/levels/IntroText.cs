using Godot;

public partial class IntroText : CanvasLayer
{
	private Panel panel = null!;
	private Label label = null!;
	private Button closeButton = null!;

	[Export] private string LabelText = ""; // Inspector-Wert
	[Export] private string ButtonText = "Weiter"; // Button-Text im Inspector

	public override void _Ready()
	{
		// Referenzen holen
		panel = GetNode<Panel>("Panel");
		label = panel.GetNode<Label>("Label");
		closeButton = panel.GetNode<Button>("Close");

		// Falls LabelText nicht im Inspector gesetzt wurde, den bestehenden Text im Label verwenden
		if (string.IsNullOrEmpty(LabelText))
		{
			LabelText = label.Text; // Übernimmt den Text, der im Label-Node im Editor gesetzt wurde
		}
		else
		{
			label.Text = LabelText; // Text aus dem Inspector setzen
		}

		// Text für den Button setzen
		closeButton.Text = ButtonText;

		// Weitere UI-Anpassungen
		panel.AnchorLeft = 0.5f;
		panel.AnchorTop = 0.5f;
		panel.AnchorRight = 0.5f;
		panel.AnchorBottom = 0.5f;

		panel.OffsetLeft = -300; // Breite 300
		panel.OffsetTop = -200; // Höhe 200
		panel.OffsetRight = 300;
		panel.OffsetBottom = 200;

		label.AnchorLeft = 0.1f;
		label.AnchorTop = 0.1f;
		label.AnchorRight = 0.9f;
		label.AnchorBottom = 0.5f;

		closeButton.AnchorLeft = 0.3f;
		closeButton.AnchorTop = 0.6f;
		closeButton.AnchorRight = 0.7f;
		closeButton.AnchorBottom = 0.8f;

		// Button-Signal verbinden
		closeButton.Pressed += OnClosePressed;

		// Spiel pausieren und Panel sichtbar machen
		GetTree().Paused = true;
		panel.Visible = true;
	}

	private void OnClosePressed()
	{
		panel.Visible = false; // Panel verstecken
		GetTree().Paused = false; // Spiel fortsetzen
	}
}
