using Godot;

namespace Game.Scenes.Levels;

public partial class AudioManager : Node
{
    [Export] private AudioStreamPlayer? _menuMusic;
    [Export] private AudioStreamPlayer? _gameMusic;
    [Export] private AudioStreamPlayer? _gameOverMusic;

    public override void _Ready()
    {
        _menuMusic = GetNode<AudioStreamPlayer>("intro_music");
        _gameMusic = GetNode<AudioStreamPlayer>("game_music");
        _gameOverMusic = GetNode<AudioStreamPlayer>("game_over");

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
