using System;
using System.Text.RegularExpressions;

using Game.Scripts;

using Godot;

public partial class Ally : CharacterBody2D
{
    [Export] Chat _chat;
    [Export] RichTextLabel _responseField;
    [Export] PathFindingMovement _pathFindingMovement;

    private bool _followPlayer = true;
    private int _motivation;
    private Player _player;

    public override void _Ready()
    {
        _chat.ResponseReceived += HandleResponse;
        _player = GetNode<Player>("%Player");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_followPlayer)
        {
            _pathFindingMovement.TargetPosition = _player.GlobalPosition;
        }
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
        Match match = regex.Match(response);

        if (match.Success && match.Groups.Count > 1)
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
