using Godot;
using System;

namespace PawsOfDestiny.Scripts;

public partial class Key : Area2D
{
	// Called when the node enters the scene tree for the first time.
	private void OnBodyEntered(Node2D body)
	{
		GD.Print("KEY + 1!");
		QueueFree();
	}
}
