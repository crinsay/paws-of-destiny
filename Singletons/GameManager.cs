using Godot;
using PawsOfDestiny.Scripts.Common;
using PawsOfDestiny.Scripts.Enemies.MeowolasEnemyComponents;
using PawsOfDestiny.Scripts.Game.Collectables.KeyComponents;
using PawsOfDestiny.Scripts.Game.GameManagerComponents;
using PawsOfDestiny.Scripts.Game;
using PawsOfDestiny.Scripts.PlayerComponents;
using System;
using System.Collections.Generic;
using System.Runtime;

namespace PawsOfDestiny.Singletons;

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

    private List<PackedScene> _levelScenes;

    private int _numberOfLevels = 6;
    private int _currentLevel = 0;
    public override void _Ready()
    {
        _keyCounter = GetNode<KeyCounter>(GameManagerConstants.KeyCounter);
        _playerHealth = GetNode<PlayerHealth>(GameManagerConstants.PlayerHealth);
        _playerHitTimer = GetNode<Timer>(GameManagerConstants.PlayerHitTimer);
        _playerDeathTimer = GetNode<Timer>(GameManagerConstants.PlayerDeathTimer);

        _levelScenes = [];

        for (int i = 0; i <= _numberOfLevels; i++)
        {
            var level = GD.Load<PackedScene>($"res://Scenes/Level{i}.tscn");
            _levelScenes.Add(level);
        }
    }

    private void SetNextLevel()
    {
        _currentLevel++;
        if (_currentLevel > _numberOfLevels)
        {
            _currentLevel = 0;
        }
    }

    public void LoadNextLevel()
    {
        //if (_collectedKeys == 3)
        //{
            _collectedKeys = 0;
            _keyCounter.UpdateKeyCounter(_collectedKeys);
            SetNextLevel();
            GetTree().ChangeSceneToPacked(_levelScenes[_currentLevel]);
        //}
    }


    //Method that handles Signal emmited by a single key:
    public void OnKeyCollected()
    {
        _collectedKeys += 1;
        _keyCounter.UpdateKeyCounter(_collectedKeys);
    }

    public void OnMeowolasEnemyNewArrowInstantiated(Node2D newArrow)
    {
        var arrow = newArrow as MeowolasArrow;

        arrow.Connect(MeowolasArrow.SignalName.EnemyHitPlayer,
            new Callable(this, nameof(OnEnemyHitPlayer)));
    }

    public void OnEnemyHitPlayer(HitInformation hitInfo) //Body is an enemy object that hit the player (for example MewolasArrow)
    {
        var player = hitInfo.Body as Player;
        if (!player.CanBeHit)
        {
            return;
        }

        EmitSignal(SignalName.EnemyHitPlayer, hitInfo);
        _playerHealth.UpdatePlayerHealthLabel(player.Health);

        if (player.State != PlayerState.Dead)
        {
            _playerHitTimer.Start();
        }
        else
        {
            Engine.TimeScale = 0.75d;
            _playerDeathTimer.Start();
        }
    }

    public void OnPlayerHitEnemy(HitInformation hitInfo)
    {
        if (hitInfo.Body is MeowolasEnemy meowolasEnemy)
        {
            if (!meowolasEnemy.CanBeHit)
            {
                return;
            }

            EmitSignal(SignalName.PlayerHitMeowolasEnemy, hitInfo);
        }
    }

    private void OnPlayerDeathTimerTimeout()
    {
        Engine.TimeScale = 1d;

        _collectedKeys = 0;
        _keyCounter.UpdateKeyCounter(_collectedKeys);
        var DeathLevel = GD.Load<PackedScene>($"res://Scenes/LevelDeath.tscn");
        _currentLevel = 0;
        GetTree().ChangeSceneToPacked(DeathLevel);
    }

    public void OnSpikesHitPlayer(HitInformation hitInfo)
    {
        var player = hitInfo.Body as Player;
        if (!player.CanBeHit)
        {
            return;
        }

        EmitSignal(SignalName.EnemyHitPlayer, hitInfo);
        _playerHealth.UpdatePlayerHealthLabel(player.Health);
        if (player.State != PlayerState.Dead)
        {
            GetNode<Timer>("PlayerHitBySpikeTimer").Start();
        }
        else
        {
            _playerDeathTimer.Start();
        }
    }

    private void OnPlayerHitBySpikeTimerTimeout()
    {
        GetTree().ReloadCurrentScene();
        _collectedKeys = 0;
        _keyCounter.UpdateKeyCounter(_collectedKeys);
    }
}
