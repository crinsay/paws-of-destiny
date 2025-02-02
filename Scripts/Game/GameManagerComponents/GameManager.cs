using Godot;
using PawsOfDestiny.Scripts.Enemies.MeowolasEnemyComponents;
using PawsOfDestiny.Scripts.Game.Collectables.KeyComponents;
using PawsOfDestiny.Scripts.PlayerComponents;
using System;
using System.Collections.Generic;

namespace PawsOfDestiny.Scripts.Game.GameManagerComponents;

public partial class GameManager : Node
{
    [Signal]
    public delegate void EnemyHitPlayerEventHandler(int damage);
    [Signal]
    public delegate void PlayerHitMeowolasEnemyEventHandler(int damage);

    private KeyCounter _keyCounter;
    private PlayerHealth _playerHealth;
    private Timer _playerHitTimer;
    private Timer _playerDeathTimer;

    private int _collectedKeys;

    public override void _Ready()
    {
        _keyCounter = GetNode<KeyCounter>(GameManagerConstants.KeyCounter);
        _playerHealth = GetNode<PlayerHealth>(GameManagerConstants.PlayerHealth);
        _playerHitTimer = GetNode<Timer>(GameManagerConstants.PlayerHitTimer);
        _playerDeathTimer = GetNode<Timer>(GameManagerConstants.PlayerDeathTimer);
    }


    //Method that handles Signal emmited by a single key:
    private void OnKeyCollected()
    {
        _collectedKeys += 1;
        _keyCounter.UpdateKeyCounter(_collectedKeys);
    }

    private void OnMeowolasEnemyNewArrowInstantiated(Node2D newArrow)
    {
        var arrow = newArrow as MeowolasArrow;

        arrow.Connect(MeowolasArrow.SignalName.EnemyHitPlayer,
            new Callable(this, nameof(OnEnemyHitPlayer)));
    }

    public void OnEnemyHitPlayer(int damage) //Body is an enemy object that hit the player (for example MewolasArrow)
    {
        if (!Player.CanBeHit)
        {
            return;
        }

        EmitSignal(SignalName.EnemyHitPlayer, damage);
        _playerHealth.UpdatePlayerHealthLabel(Player.Health);

        if (Player.State != PlayerState.Dead)
        {
            _playerHitTimer.Start();
        }
        else
        {
            Engine.TimeScale = 0.75d;
            _playerDeathTimer.Start();
        }
    }

    private void OnPlayerHitEnemy(Node2D enemy, int damage)
    {
        if (enemy is MeowolasEnemy meowolasEnemy)
        {
            if (!meowolasEnemy.CanBeHit)
            {
                return;
            }

            EmitSignal(SignalName.PlayerHitMeowolasEnemy, damage);
        }
    }

    private void OnPlayerDeathTimerTimeout()
    {
        Engine.TimeScale = 1d;
        GetTree().ReloadCurrentScene();
    }


    private void OnDoorsBodyEntered(Node2D body)
    {
        var gameManagerSingleton = GD.Load<GameManagerSingleton>("res://Singletons/GameManagerSingleton.cs");
        gameManagerSingleton.LoadNextLevel();
    }

}
