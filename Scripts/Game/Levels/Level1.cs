using Godot;
using PawsOfDestiny.Scripts.PlayerComponents;
using PawsOfDestiny.Singletons;
using System;

namespace PawsOfDestiny.Scripts.Game.Levels;

public partial class Level1 : Node2D
{
    private GameManager _gameManager;
    public override void _Ready()
	{
        _gameManager = GetNode<GameManager>("/root/GameManager");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnMeowolasFightStartBodyEntered(Node2D body)
	{
		if (body is Player)
		{
			_gameManager.OnLevelMeowolasEnemyFightStart();
		}
	}
}
