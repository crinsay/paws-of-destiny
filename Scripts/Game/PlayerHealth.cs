using Godot;
using PawsOfDestiny.Scripts.Game.Collectables.KeyComponents;
using System;

namespace PawsOfDestiny.Scripts.Game;
public partial class PlayerHealth : CanvasLayer
{
    private Label _playerHealthValue;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        _playerHealthValue = GetNode<Label>("PlayerHealthValue");
        _playerHealthValue.Text = "Health: 9";
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}


    public void UpdatePlayerHealthLabel(int health)
    {
        _playerHealthValue.Text = $"Health: {health}";
    }
}
