using System;

using Godot;

public partial class Motivation : Node
{
    [Signal] public delegate void UnmotivatedEventHandler();
    [Signal] public delegate void MotivationChangedEventHandler(int newMotivation);

    [Export] public int MaxMotivation { get; private set; } = 10;

    [Export] public int Amount { get; set; } = 10;
    private int _frame = 0;

    public void SetMotivation(int motivation)
    {
        motivation = Math.Min(MaxMotivation, Math.Max(0, motivation));
        Amount = motivation;
        EmitSignal(SignalName.MotivationChanged, Amount);
    }
}
