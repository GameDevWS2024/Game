using Game.Scenes.Levels;
using Game.Scripts;

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
    private AudioStreamPlayer? _damageSound;
    private Ally _ally = null!;
    private ButtonControl _buttonControl = null!;

    public override void _Ready()
    {
        _damageSound = GetTree().Root.GetNode<AudioStreamPlayer>("Node2D/AudioManager/damage_sound");
        _ally = this.GetParent<Ally>();
        _buttonControl = GetTree().Root.GetNode<ButtonControl>("Node2D/UI");
    }

    public void Heal(double amount)
    {
        if ((!Dead || _reviveable) && Amount < MaxHealth)
        {
            Amount += amount;
            GD.Print(Amount);
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
        GD.Print(Amount);

        //Sound part
        if (_ally.Name == "Ally" && _buttonControl.CurrentCamera == 1 || _ally.Name == "Ally2" && _buttonControl.CurrentCamera == 2)
        {
            _damageSound.Play();
        }

        if (Amount < 0)
        {
            Dead = true;
            EmitSignal(SignalName.Death);
            GD.Print("died");
            Amount = 0;
        }

        EmitSignal(SignalName.HealthChanged, Amount);
    }

}
