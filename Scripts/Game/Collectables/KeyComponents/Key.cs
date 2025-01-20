using Godot;
using PawsOfDestiny.Scripts.Game.GameManagerComponents;
using System;

namespace PawsOfDestiny.Scripts.Game.Collectables.KeyComponents;

public partial class Key : Area2D
{
    private GameManager _gameManager;

    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>(GameUniqueNames.GameManager);
    }
    private void OnBodyEntered(Node2D body)
	{
        _gameManager.AddScore();
		QueueFree();
	}
}
