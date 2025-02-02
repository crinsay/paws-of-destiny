using Godot;
using PawsOfDestiny.Scripts.Game.Collectables.KeyComponents;
using PawsOfDestiny.Scripts.Game.GameManagerComponents;
using PawsOfDestiny.Scripts.Game;
using System;
using System.Collections.Generic;
using PawsOfDestiny.Scripts.Enemies.MeowolasEnemyComponents;
using PawsOfDestiny.Scripts.PlayerComponents;

public partial class GameManagerSingleton : Node
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

    private PackedScene _gamePackedScene; //TODO: change it to main menu screen
    private List<PackedScene> _levelScenes;

    private int _numberOfLevels = 5;
    private int _currentLevel = 0;
    public override void _Ready()
    {
        _keyCounter = GetNode<KeyCounter>(GameManagerConstants.KeyCounter);
        _playerHealth = GetNode<PlayerHealth>(GameManagerConstants.PlayerHealth);
        _playerHitTimer = GetNode<Timer>(GameManagerConstants.PlayerHitTimer);
        _playerDeathTimer = GetNode<Timer>(GameManagerConstants.PlayerDeathTimer);

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
    private void OnKeyCollected()
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
        var gameManagerSingleton = GetNode<GameManagerSingleton>("/root/GameManagerSingleton");
        gameManagerSingleton.LoadNextLevel();
    }

    //public void DoorsCreated(Area2D area)
    //{
    //    if (area is Doors door)
    //    {
    //        door.Connect(Doors.SignalName.PlayerEntered,
    //           new Callable(this, nameof(OnDoorsBodyEntered)));
    //    }
    //}

}
