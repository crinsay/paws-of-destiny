using Godot;
using PawsOfDestiny.Scripts.Constants;
using System;

namespace PawsOfDestiny.Scripts;

public partial class GameManager : Node
{
    private Label _scoreLabel;
    private int _score;

    public override void _Ready()
    {
        _scoreLabel = GetNode<Label>("ScoreLabel");
    }

    public void AddScore()
    {
        _score += 1;
        _scoreLabel.Text = $"Collected keys: {_score}";
    }
}
