using Godot;
using PawsOfDestiny.Scripts.Common;
using PawsOfDestiny.Scripts.Common.MeowtarTheBlueComponents;
using PawsOfDestiny.Scripts.Enemies.MeowolasEnemyComponents;
using PawsOfDestiny.Scripts.Enemies.MeowtarTheBlueComponents;
using PawsOfDestiny.Scripts.Game;
using PawsOfDestiny.Scripts.Game.Collectables.KeyComponents;
using PawsOfDestiny.Scripts.PlayerComponents;
using System.Collections.Generic;

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
    [Signal]
    public delegate void PlayerCollectHeartEventHandler();

    private KeyCounter _keyCounter;
    private PlayerHealth _playerHealth;
    private Timer _meowolasEnemyAndPlayerFightTimer;
    private Timer _playerDeathTimer;

    private int _collectedKeys;

    private PackedScene _gamePackedScene; //TODO: change it to main menu screen
    private List<PackedScene> _levelScenes;

    private int _numberOfLevels = 6;
    private int _currentLevel = 0;
    public override void _Ready()
    {
        _keyCounter = GetNode<KeyCounter>("KeyCounter");
        _playerHealth = GetNode<PlayerHealth>("PlayerHealth");
        _meowolasEnemyAndPlayerFightTimer = GetNode<Timer>("MeowolasEnemyAndPlayerFightTimer");
        _playerDeathTimer = GetNode<Timer>("PlayerDeathTimer");

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
            _currentLevel = 1;
        }
    }

    public void LoadNextLevel()
    {
        if (_collectedKeys == 3)
        {
            _collectedKeys = 0;
            _keyCounter.UpdateKeyCounter(_collectedKeys);
            SetNextLevel();
            GetTree().ChangeSceneToPacked(_levelScenes[_currentLevel]);
        }
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

    public void OnLevelMeowolasEnemyFightStart()
    {
        _meowolasEnemyAndPlayerFightTimer.Start();
    }

    private void OnMeowolasEnemyAndPlayerFightTimerTimeout()
    {
        GD.Print("End of fight!");
        EmitSignal(SignalName.MeowolasEnemyRunAway);
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

    public void OnHeartCollected(Node2D body)
    {
        var player = body as Player;
        EmitSignal(SignalName.PlayerCollectHeart);
        _playerHealth.UpdatePlayerHealthLabel(player.Health);
    }

    public void PlayerHealthReset()
    {
        if (_collectedKeys == 3)
        {
            GetNode<PlayerStats>("/root/PlayerStats").Health = 9;
            _playerHealth.UpdatePlayerHealthLabel(9);
        }
    }
}
