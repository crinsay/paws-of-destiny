using Godot;
using System;

namespace PawsOfDestiny.Scripts.Game.GameManagerComponents;

public partial class GameManager : Node
{
    private Label _scoreLabel;

    private int _score;

    public override void _Ready()
    {
        _scoreLabel = GetNode<Label>(GameManagerConstants.ScoreLabel);
    }

    public void AddScore()
    {
        _score += 1;
        _scoreLabel.Text = $"Collected keys: {_score}";
    }
}
