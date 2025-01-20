using Godot;
using PawsOfDestiny.Scripts.Constants;
using System;

namespace PawsOfDestiny.Scripts;

public partial class Key : Area2D
{
    private GameManager _gameManager;

    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("%GameManager");
    }
    private void OnBodyEntered(Node2D body)
	{
        _gameManager.AddScore();
		QueueFree();
	}
}
