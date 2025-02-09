using Godot;
public partial class CodeInput : Node2D
{
    private LineEdit _codeinputfield = null!;
    private AnimationPlayer _fadeToWhite = null!;
    private Node2D _victoryScreen = null!;
    public override void _Ready()
    {
        _codeinputfield = GetTree().Root.GetNode<LineEdit>("Node2D/RuneHolder/CodeInputField");
        _codeinputfield.TextSubmitted += OnInputSubmitted;
        _fadeToWhite = GetTree().Root.GetNode<AnimationPlayer>("Node2D/ColorRect/FadeToWhite");
    }
    public override void _PhysicsProcess(double delta)
    {
        Interactable interactable = GetParent().GetNode<Interactable>("Interactable");
        if (interactable != null)
        {
            interactable.Interact += OpenTextField;
        }

    }

    private void OnInputSubmitted(string text)
    {
        if (text.Contains("1234"))
        {
            GD.Print("Game Won");
            _fadeToWhite.Play("fade_to_white");
        }
    }

    private void OpenTextField()
    {
        _codeinputfield.Visible = true;
        _codeinputfield.GrabFocus();
    }
}
