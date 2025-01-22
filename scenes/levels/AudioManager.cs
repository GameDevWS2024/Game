using Godot;

namespace Game.scenes.levels;

public partial class AudioManager : Node
{
    [Export] private AudioStreamPlayer? _menuMusic;
    [Export] private AudioStreamPlayer? _gameMusic;
    [Export] private AudioStreamPlayer? _gameOverMusic;

    public override void _Ready()
    {
        _menuMusic = GetNode<AudioStreamPlayer>("MenuMusic");
        _gameMusic = GetNode<AudioStreamPlayer>("GameMusic");
        _gameOverMusic = GetNode<AudioStreamPlayer>("GameOverMusic");

        // Start menu music initially
        _menuMusic.Play();
    }

    public void StartGameMusic()
    {
        _menuMusic!.Stop();
        _gameMusic!.Play();
    }

    public void PlayGameOverMusic()
    {
        _gameMusic!.Stop();
        _gameOverMusic!.Play();
    }
}