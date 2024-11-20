using Game.Scripts;
using System.Text.RegularExpressions;
using System;

using Godot;

public partial class Ally : Node2D
{
    [Export] PathFindingComponent _pathFinder;
    [Export] Chat _chat;
    [Export] RichTextLabel _responseField;

    bool _followPlayer = true;
    int _motivation;

    Player _player;


    public override void _Ready()
    {
        _chat.ResponseReceived += HandleResponse;
        _player = GetNode<Player>("%Player");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_followPlayer) {
            _pathFinder.GoTo(_player.GlobalPosition);
        }

        GlobalPosition = _pathFinder.GlobalPosition;
    }

    private void HandleResponse(string response)
    {
        if (_responseField != null)
        {
            _responseField.Text = response;
        }
        GD.Print($"Response: {response}");

        string pattern = @"MOTIVATION:\s*(\d+)";
        Regex regex = new Regex(pattern);
        Match match = regex.Match(response);  // Match against responsePice, not pattern

        if (match.Success && match.Groups.Count > 1)  // Check match.Success
        {
            try
            {
                _motivation = int.Parse(match.Groups[1].Value);
            }
            catch (Exception ex)
            {
                GD.Print(ex);
            }
        }

        if (response.Contains("FOLLOW"))
        {
            GD.Print("following");
            _followPlayer = true;
        }

        if (response.Contains("STOP"))
        {
            GD.Print("stop");
            _followPlayer = false;
        }

        GD.Print($"Motivation: {_motivation}");
    }
}
