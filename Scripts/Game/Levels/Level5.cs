using Godot;
using PawsOfDestiny.Singletons;
using System;

namespace PawsOfDestiny.Scripts.Game;
public partial class Level5 : Node2D
{
	[Signal]
	public delegate void ItsBossFightTimeEventHandler();

	private GameManager _gameManager;
	public override void _Ready()
	{
        _gameManager = GetNode<GameManager>("/root/GameManager");

        EmitSignal(SignalName.ItsBossFightTime);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
