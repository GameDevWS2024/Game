using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

using Game.Scripts;

using Godot;

public partial class Ally : CharacterBody2D
{
    [Export] Chat _chat = null!;
    [Export] RichTextLabel _responseField = null!;
    [Export] PathFindingMovement _pathFindingMovement = null!;

    private bool _followPlayer = true;
    private int _motivation;
    private Player _player = null!;

    public int _healthPoints = 100;
    public bool _allyInDarkness = false;
    public bool _allyInSmallCircle = true;
    public bool _allyInBigCircle = true;

    private Core _core;

    public override void _Ready()
    {
        _chat.ResponseReceived += HandleResponse;
        _player = GetNode<Player>("%Player");
        _core = GetNode<Core>("%Core");  // Core einmal speichern
    }

    public void _setAllyInDarkness()
    {
        // Berechne den Abstand zwischen Ally und Core
        Vector2 _distance = this.Position - _core.Position;
        float _distanceLength = _distance.Length();  // Berechne die Länge des Vektors

        // If ally further away than big circle, he is in the darkness
        if (_distanceLength > _core.LightRadius * 1.5f)
        {
            this._allyInDarkness = true;
            this._allyInSmallCircle = false;
            this._allyInBigCircle = false;
        }
        //if ally not in darkness and closer than the small Light Radius, he is in small circle
        else if (_distanceLength < _core.LightRadius)
        {
            this._allyInDarkness = false;
            this._allyInSmallCircle = true;
            this._allyInBigCircle = false;
            // Korrektur der falschen Referenz
        }
        //if ally not in darkness and not in small circle, ally is in big circle
        else
        {
            this._allyInDarkness = false;
            this._allyInSmallCircle = false;
            this._allyInBigCircle = true;
            // Korrektur der falschen Referenz
        }

    }

    public override void _PhysicsProcess(double delta)
    {
        _setAllyInDarkness();

        if (_followPlayer)
        {
            _pathFindingMovement.TargetPosition = _player.GlobalPosition;
        }
    }

    private void HandleResponse(string response)
    {

        _responseField.Text = response;

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
