using Godot;
using PawsOfDestiny.Singletons;
using System;

namespace PawsOfDestiny.Scripts.Game;

public partial class PlayerResetStats : Area2D
{
    private GameManager _gameManager;

    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager");
    }


    private void OnBodyEntered(Node2D _)
    {
        _gameManager.HealthReset();
    }
}
