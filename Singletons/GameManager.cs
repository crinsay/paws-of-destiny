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
    private Timer _meowolasEnemyAndPlayerFightTimer;
    private Timer _playerDeathTimer;
    private AudioStreamPlayer _audioStreamPlayer;
    private AudioStreamPlayer _keyPickingSound;
    private AudioStreamPlayer _heartPickingSound;
    private AudioStreamPlayer _deathSound;
    private AudioStreamPlayer _hitSound;

    private int _collectedKeys;

    private List<PackedScene> _levelScenes;

    private int _numberOfLevels = 6;
    private int _currentLevel = 0;
    public override void _Ready()
    {
        _keyCounter = GetNode<KeyCounter>("KeyCounter");
        _meowolasEnemyAndPlayerFightTimer = GetNode<Timer>("MeowolasEnemyAndPlayerFightTimer");
        _playerDeathTimer = GetNode<Timer>("PlayerDeathTimer");
        _audioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        _keyPickingSound = GetNode<AudioStreamPlayer>("KeyPickingSound");
        _heartPickingSound = GetNode<AudioStreamPlayer>("HeartPickingSound");
        _deathSound = GetNode<AudioStreamPlayer>("DeathSound");
        _hitSound = GetNode<AudioStreamPlayer>("HitSound");

        _levelScenes = [];

        for (int i = 0; i <= _numberOfLevels; i++)
        {
            var level = GD.Load<PackedScene>($"res://Scenes/Level{i}.tscn");
            _levelScenes.Add(level);
        }

        _audioStreamPlayer.Play();
    }

    private void SetNextLevel()
    {
        _currentLevel++;
        if (_currentLevel == 5)
        {
            _audioStreamPlayer.Stop();
        }
        
        if (_currentLevel > _numberOfLevels)
        {
            PlayerHealthReset();
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
        _keyPickingSound.Play();
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
        _hitSound.Play();
        EmitSignal(SignalName.EnemyHitPlayer, hitInfo);

        if (player.State == PlayerState.Dead)
        {
            Engine.TimeScale = 0.75d;
            _deathSound.Play();
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
                EmitSignal(SignalName.MeowolasEnemyRunAway);
                _meowolasEnemyAndPlayerFightTimer.Stop();

                _currentLevel = 5;
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
        EmitSignal(SignalName.MeowolasEnemyRunAway);
    }

    private void OnPlayerDeathTimerTimeout()
    {
        Engine.TimeScale = 1d;
        _collectedKeys = 0;
        _keyCounter.UpdateKeyCounter(_collectedKeys);
        var DeathLevel = GD.Load<PackedScene>($"res://Scenes/LevelDeath.tscn");
        _currentLevel = 0;
        _audioStreamPlayer.Stop(); 
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
        _heartPickingSound.Play();
        var player = body as Player;
        EmitSignal(SignalName.PlayerCollectHeart);
    }

    public void PlayerHealthReset()
    {
        if (_collectedKeys == 3)
        {
            GetNode<PlayerStats>("/root/PlayerStats").Health = 9;
            _audioStreamPlayer.Play();
        }
    }
}
