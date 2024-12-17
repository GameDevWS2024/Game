using Godot;

public partial class IntroText : CanvasLayer
{
    [Export] private Button _closeButton = null!;

    public override void _Ready()
    {
        _closeButton.Pressed += OnClosePressed;

        // Spiel pausieren und Panel sichtbar machen
        GetTree().Paused = true;
        Visible = true;
    }

    private void OnClosePressed()
    {
        Visible = false; // Panel verstecken
        GetTree().Paused = false; // Spiel fortsetzen
    }
}
