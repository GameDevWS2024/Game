using Godot;
public partial class CodeInput : Node2D
{    
    private LineEdit _codeinputfield = null!;
    public override void _Ready()
    {
        _codeinputfield = GetTree().Root.GetNode<LineEdit>("Node2D/RuneHolder/CodeInputField");
        _codeinputfield.TextSubmitted += OnInputSubmitted;
    }
    public override void _PhysicsProcess(double delta)
    {
            Interactable interactable = GetParent().GetNode<Interactable>("Interactable");
            if (interactable != null)
            {
                interactable.Interact += OpenTextField;
            }

    }

    private void OnInputSubmitted(string text) {
        if(text.Contains("1234")) {
            GD.Print("Game isch vorbei");
        }
    }

    private void OpenTextField(){
        _codeinputfield.Visible = true;
        _codeinputfield.GrabFocus();
    }
}