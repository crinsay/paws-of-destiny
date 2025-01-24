using Godot;
using PawsOfDestiny.Scripts.Enemies.MeowolasEnemyComponents;
using PawsOfDestiny.Scripts.Game.Collectables.KeyComponents;
using PawsOfDestiny.Scripts.PlayerComponents;
using System;

namespace PawsOfDestiny.Scripts.Game.GameManagerComponents;

public partial class GameManager : Node
{
    [Signal]
    public delegate void MeowolasArrowHitPlayerEventHandler(int damage);

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

        arrow.Connect(MeowolasArrow.SignalName.MeowolasArrowHitPlayer,
            new Callable(this, nameof(OnMeowolasArrowHitPlayer)));
    }


    public void OnMeowolasArrowHitPlayer(int damage)
    {
        if (GameState.WasPlayerHit || GameState.IsPlayerDead)
        {
            return;
        }

        EmitSignal(SignalName.MeowolasArrowHitPlayer, damage);
        GameState.WasPlayerHit = true;

        //await ToSignal(this, SignalName.MeowolasArrowHitPlayer);

        _playerHealth.UpdatePlayerHealthLabel(Player.CurrentHealth);

        if (Player.CurrentHealth > 0)
        {
            _playerHitTimer.Start();
        }
        else
        {
            GameState.IsPlayerDead = true;
            Engine.TimeScale = 0.75d;

            _playerDeathTimer.Start();
        }
    }

    private void OnPlayerHitTimerTimeout()
    {
        GameState.WasPlayerHit = false;
    }

    private void OnPlayerDeathTimerTimeout()
    {
        Engine.TimeScale = 1d;
        GetTree().ReloadCurrentScene();

        GameState.WasPlayerHit = false;
        GameState.IsPlayerDead = false;
    }
}
