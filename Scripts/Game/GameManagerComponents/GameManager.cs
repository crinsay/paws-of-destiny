using Godot;
using PawsOfDestiny.Scripts.Game.Collectables.KeyComponents;
using PawsOfDestiny.Scripts.PlayerComponents;
using System;

namespace PawsOfDestiny.Scripts.Game.GameManagerComponents;

public partial class GameManager : Node
{
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

    private void OnPlayerHitEnemy(Node2D body) //The player is the body here.
    {
        if (body is Player player)
        {
            GameState.WasPlayerHit = true;
            _playerHealth.UpdatePlayerHealthLabel(player.Health);

            if (player.Health > 0)
            {
                _playerHitTimer.Start();
            }
            else
            {
                Engine.TimeScale = 0.75d;
                _playerDeathTimer.Start();
            }        
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
