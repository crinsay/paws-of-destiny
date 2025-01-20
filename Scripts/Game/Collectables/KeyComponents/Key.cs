using Godot;
using PawsOfDestiny.Scripts.Game.GameManagerComponents;
using System;

namespace PawsOfDestiny.Scripts.Game.Collectables.KeyComponents;

public partial class Key : Area2D
{
    [Signal]
    public delegate void KeyCollectedEventHandler();

    public override void _Ready()
    {
    }

    private void OnBodyEntered(Node2D body)
	{
        EmitSignal(SignalName.KeyCollected);
        QueueFree();
	}
}
