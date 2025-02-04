using Godot;
using System;

namespace PawsOfDestiny.Scripts.Game;
public partial class Level5 : Node2D
{
	[Signal]
	public delegate void ItsBossFightTimeEventHandler();
	public override void _Ready()
	{
		EmitSignal(SignalName.ItsBossFightTime);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
