using Godot;
using PawsOfDestiny.Scripts.Common;
using PawsOfDestiny.Scripts.Enemies.MeowolasEnemyComponents;
using PawsOfDestiny.Scripts.Game.Collectables.KeyComponents;
using PawsOfDestiny.Scripts.Game;
using PawsOfDestiny.Scripts.PlayerComponents;
using System;
using System.Collections.Generic;
using PawsOfDestiny.Scripts.Enemies.MeowtarTheBlueComponents;
using PawsOfDestiny.Scripts.Common.MeowtarTheBlueComponents;

namespace PawsOfDestiny.Singletons;

public partial class GameManager : Node
{
    [Signal]
    public delegate void EnemyHitPlayerEventHandler(HitInformation hitInfo);
    [Signal]
    public delegate void PlayerHitMeowolasEnemyEventHandler(HitInformation hitInfo);
    [Signal]
    public delegate void PlayerHitMeowtarTheBlueEnemyEventHandler(HitInformation hitInfo);
    [Signal]
    public delegate void MeowolasEnemyRunAwayEventHandler();

    private KeyCounter _keyCounter;
    private PlayerHealth _playerHealth;
    private Timer _meowolasEnemyAndPlayerFightTimer;
    private Timer _playerDeathTimer;

    private int _collectedKeys;

    private PackedScene _gamePackedScene; //TODO: change it to main menu screen
    private List<PackedScene> _levelScenes;

    private int _numberOfLevels = 5;
    private int _currentLevel = 0;
    public override void _Ready()
    {
        _keyCounter = GetNode<KeyCounter>("KeyCounter");
        _playerHealth = GetNode<PlayerHealth>("PlayerHealth");
        _meowolasEnemyAndPlayerFightTimer = GetNode<Timer>("MeowolasEnemyAndPlayerFightTimer");
        _playerDeathTimer = GetNode<Timer>("PlayerDeathTimer");

        _levelScenes = [];

        for (int i = 1; i <= _numberOfLevels; i++)
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
            _currentLevel = 1;
        }
    }

    private void LoadMainMenu()
    {
        //TODO: change it to load to main menu (starting page)
        _currentLevel = 0;
        GetTree().ChangeSceneToPacked(_gamePackedScene);
    }

    public void LoadNextLevel()
    {
        SetNextLevel();
        GetTree().ChangeSceneToPacked(_levelScenes[_currentLevel - 1]);
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

    public void OnMeowtarTheBlueEnemyNewFireballInstantiated(Node2D newFireball)
    {
        var fireball = newFireball as MeowtarTheBlueFireball;

        fireball.Connect(MeowtarTheBlueFireball.SignalName.EnemyHitPlayer,
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

        if (player.State == PlayerState.Dead)
        {
            Engine.TimeScale = 0.75d;
            _playerDeathTimer.Start();
        }        
    }

    public void OnPlayerHitEnemy(HitInformation hitInfo)
    {
        if (hitInfo.Body is MeowolasEnemy meowolas)
        {
            if (!meowolas.CanBeHit)
            {
                return;
            }

            EmitSignal(SignalName.PlayerHitMeowolasEnemy, hitInfo);

            if (meowolas.Health == 1)
            {
                GD.Print("Boss fight!");
                EmitSignal(SignalName.MeowolasEnemyRunAway);
                _meowolasEnemyAndPlayerFightTimer.Stop();
            }
        }
        else if (hitInfo.Body is MeowtarTheBlueEnemy meowtarTheBlue)
        {
            if (!meowtarTheBlue.CanBeHit)
            {  
                return; 
            }

            EmitSignal(SignalName.PlayerHitMeowtarTheBlueEnemy, hitInfo);
        }
    }

    private void OnPlayerDeathTimerTimeout()
    {
        Engine.TimeScale = 1d;
        GetTree().ReloadCurrentScene();
    }

    public void OnLevelMeowolasEnemyFightStart()
    {
        _meowolasEnemyAndPlayerFightTimer.Start();
    }

    private void OnMeowolasEnemyAndPlayerFightTimerTimeout()
    {
        GD.Print("End of fight!");
        EmitSignal(SignalName.MeowolasEnemyRunAway);
    }
}
