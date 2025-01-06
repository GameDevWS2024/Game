using Godot;

namespace Game.Scripts;

public partial class MotivationBar : ProgressBar
{
    [Export] Motivation? _motivation;

    public override void _Ready()
    {
        if (_motivation == null)
        {
            return;
        }

        _motivation.MotivationChanged += OnMotivationChange;
        Value = _motivation.Amount;
    }

    public void OnMotivationChange(int newMotivation)
    {
        if (_motivation != null)
        {
            Value = (10 * newMotivation) / _motivation.MaxMotivation;
        }
    }


}