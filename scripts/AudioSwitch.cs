using System;

using Game.Scripts;

using Godot;

public partial class AudioSwitch : Node2D
{
    [Export] public AudioStreamPlayer MusicPlayer1 = null!; // Aktuelle Musik
    [Export] public AudioStreamPlayer MusicPlayer2 = null!; // Neue Musik
    [Export] public float FadeDuration = 2.0f;
    [Export] public float CityRadius = 3000f;

    private bool _areAlliesInsideCity = false;
    private bool _isFading = false;
    private bool _isInside = false;
    private Ally _ally1 = null!;
    private Ally _ally2 = null!;

    private bool _musicAlreadyPlaying = false;

    public override void _Ready()
    {
        _ally1 = GetTree().Root.GetNode<Ally>("Node2D/Ally");
        _ally2 = GetTree().Root.GetNode<Ally>("Node2D/Ally2");
    }

    public override void _PhysicsProcess(double delta)
    {
        _areAlliesInsideCity = AreAlliesInsideCity();

        //GD.Print("ally inside city: " + _areAlliesInsideCity + "  creepy music playing: " + _musicAlreadyPlaying);
        if (_areAlliesInsideCity && !_musicAlreadyPlaying)
        {
            FadeMusic(MusicPlayer1, MusicPlayer2);
            _musicAlreadyPlaying = true;
        }
        else if (_areAlliesInsideCity && _musicAlreadyPlaying)
        {
            _musicAlreadyPlaying = false;
            FadeMusic(MusicPlayer2, MusicPlayer1);
        }
    }

    public bool AreAlliesInsideCity()
    {
        Vector2 distance1 = _ally1.GlobalPosition - this.GlobalPosition;
        float distanceLength1 = distance1.Length();
        Vector2 distance2 = _ally2.GlobalPosition - this.GlobalPosition;
        float distanceLength2 = distance2.Length();

        if (distanceLength1 < CityRadius || distanceLength2 < CityRadius)
        {
            return true;
        }
        return false;
    }

    private async void FadeMusic(AudioStreamPlayer fadeOutPlayer, AudioStreamPlayer fadeInPlayer)
    {
        if (_isFading)
        {
            return;
        }

        _isFading = true;

        float initialVolume = fadeOutPlayer.VolumeDb;
        float targetVolume = -30f; // Leise werden

        float step = (targetVolume - initialVolume) / (FadeDuration * 60);

        // Fade-Out der aktuellen Musik
        for (float i = initialVolume; i > targetVolume; i += step)
        {
            fadeOutPlayer.VolumeDb = i;
            await ToSignal(GetTree().CreateTimer(1f / 60f), "timeout");
        }

        fadeOutPlayer.Stop();
        fadeInPlayer.Play();

        // Fade-In der neuen Musik
        fadeInPlayer.VolumeDb = targetVolume;
        step = (initialVolume - targetVolume) / (FadeDuration * 60);

        for (float i = targetVolume; i < initialVolume; i += step)
        {
            fadeInPlayer.VolumeDb = i;
            await ToSignal(GetTree().CreateTimer(1f / 60f), "timeout");
        }

        _isFading = false;
    }
}
