using Godot;

public partial class HealthBar : ProgressBar 
{
    [Export]
    Health? _health;

    public override void _Ready()
    {
        if (_health != null)
        {
            _health.HealthChanged += OnHealthChange;
            Value = _health.Amount;
        }
    }

    public void OnHealthChange(int newHealth) 
    {
        if (_health != null)
        {
            Value = newHealth / _health.MaxHealth * 100;
        }
    }


}