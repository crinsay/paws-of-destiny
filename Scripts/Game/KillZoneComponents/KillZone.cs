using Godot;
using PawsOfDestiny.Scripts.PlayerComponents;
using System;

namespace PawsOfDestiny.Scripts.Game.KillZoneComponents;

public partial class KillZone : Area2D
{
    private Timer _timer;

    public override void _Ready()
    {
        _timer = GetNode<Timer>(KillZoneConstants.Timer);
    }


    private void OnBodyEntered(Node2D body) //The only body that can enter KillZone is Player
    {
        if (!GameState.IsPlayerDead && body is Player player)
        {
            GameState.IsPlayerDead = true;

            Engine.TimeScale = 0.75d;

            player.PlayAnimation(PlayerConstants.Animations.Death);

            _timer.Start();
        }
    }

    private void OnTimerTimeout()
    {
        Engine.TimeScale = 1d;
        GetTree().ReloadCurrentScene();

        GameState.IsPlayerDead = false;
    }
}
