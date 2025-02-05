using Godot;
using PawsOfDestiny.Scripts.Enemies.MeowolasEnemyComponents;
using PawsOfDestiny.Scripts.PlayerComponents;
using PawsOfDestiny.Singletons;
using System;

namespace PawsOfDestiny.Scripts.Common;

public partial class Doors : Area2D
{
    private GameManager _gameManager;

    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager");
    }

    private void OnDoorsBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            _gameManager.CallDeferred(nameof(_gameManager.LoadNextLevel));
        }
        else if (body is MeowolasEnemy meowolas && meowolas.State == MeowolasState.RunAway)
        {
            meowolas.QueueFree();
        }
    }

    private void OnWorldItsBossFightTime()
    {
        _gameManager.Connect(GameManager.SignalName.MeowolasAndMeowtarDefeated,
                new Callable(this, nameof(OnMeowolasAndMeowtarDefeated)));

        GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
    }

    private void OnMeowolasAndMeowtarDefeated()
    {
        GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
    }
}
