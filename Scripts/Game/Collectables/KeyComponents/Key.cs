using Godot;
using PawsOfDestiny.Scripts.Game.GameManagerComponents;
using PawsOfDestiny.Scripts.PlayerComponents;
using PawsOfDestiny.Singletons;
using System;

namespace PawsOfDestiny.Scripts.Game.Collectables.KeyComponents;

public partial class Key : Area2D
{
    [Signal]
    public delegate void KeyCollectedEventHandler();

    private GameManager _gameManager;

    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager");
    }

    private void OnBodyEntered(Node2D body)
	{
        if (body is Player)
        {
            _gameManager.OnKeyCollected();
            QueueFree();
        }
	}
}
