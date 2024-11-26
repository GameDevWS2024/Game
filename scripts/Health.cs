using Godot;

public partial class Health : Node
{
    [Signal] public delegate void DeathEventHandler();
    [Signal] public delegate void HealthChangedEventHandler(int newHealth);

    [Export] private bool _reviveable;

    [Export] public double MaxHealth { get; private set; } = 100;

    [Export] public double Amount { get; private set; } = 100;
    private int _frame = 0;

    public bool Dead { get; private set; } = false;

    public void Heal(double amount)
    {
        if ((!Dead || _reviveable) && Amount < MaxHealth)
        {
            Amount += amount;
            if (Amount > MaxHealth)
            {
                Amount = MaxHealth;
            }

            EmitSignal(SignalName.HealthChanged, Amount);
        }
    }

    public void Damage(double amount)
    {
        Amount -= amount;

        if (Amount < 0)
        {
            Dead = true;
            EmitSignal(SignalName.Death);
            Amount = 0;
        }

        EmitSignal(SignalName.HealthChanged, Amount);
    }

}
