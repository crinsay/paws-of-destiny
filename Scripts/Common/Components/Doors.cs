using Godot;
using PawsOfDestiny.Singletons;
using System;

namespace PawsOfDestiny.Scripts.Common;

public partial class Doors : Area2D
{
    private GameManager _gameManager;

    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager");
        //gameManagerSingleton.DoorsCreated(this);
    }

    private void OnDoorsBodyEntered(Node2D _)
    {
        GD.Print("on area entered");
        _gameManager.CallDeferred(nameof(_gameManager.LoadNextLevel));
    }
}
