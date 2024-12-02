using Godot;

public partial class IntroText : CanvasLayer
{
	private Panel _panel = null!;
	private Label _label = null!;
	private Button _closeButton = null!;

	[Export] private string LabelText = ""; // Inspector-Wert für den Label-Text
	[Export] private string ButtonText = ""; // Inspector-Wert für den Button-Text

	public override void _Ready()
	{
		// Referenzen holen
		_panel = GetNode<Panel>("Panel");
		_label = _panel.GetNode<Label>("Label");
		_closeButton = _panel.GetNode<Button>("Close");

		// Text für Label setzen
		if (string.IsNullOrEmpty(LabelText))
		{
			LabelText = _label.Text; // Übernimmt den Text, der im Label-Node im Editor gesetzt wurde
		}
		else
		{
			_label.Text = LabelText; // Text aus dem Inspector setzen
		}

		// Text für Button setzen
		if (string.IsNullOrEmpty(ButtonText))
		{
			ButtonText = _closeButton.Text; // Übernimmt den Text, der im Button-Node im Editor gesetzt wurde
		}
		else
		{
			_closeButton.Text = ButtonText; // Text aus dem Inspector setzen
		}

		// Weitere UI-Anpassungen
		_panel.AnchorLeft = 0.5f;
		_panel.AnchorTop = 0.5f;
		_panel.AnchorRight = 0.5f;
		_panel.AnchorBottom = 0.5f;

		_panel.OffsetLeft = -300; // Breite 300
		_panel.OffsetTop = -200; // Höhe 200
		_panel.OffsetRight = 300;
		_panel.OffsetBottom = 200;

		_label.AnchorLeft = 0.1f;
		_label.AnchorTop = 0.1f;
		_label.AnchorRight = 0.9f;
		_label.AnchorBottom = 0.5f;

		_closeButton.AnchorLeft = 0.3f;
		_closeButton.AnchorTop = 0.6f;
		_closeButton.AnchorRight = 0.7f;
		_closeButton.AnchorBottom = 0.8f;

		// Button-Signal verbinden
		_closeButton.Pressed += OnClosePressed;

		// Spiel pausieren und Panel sichtbar machen
		GetTree().Paused = true;
		_panel.Visible = true;
	}

	private void OnClosePressed()
	{
		_panel.Visible = false; // Panel verstecken
		GetTree().Paused = false; // Spiel fortsetzen
	}
}
