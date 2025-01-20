using Godot;
using PawsOfDestiny.Scripts.Constants;
using System;

namespace PawsOfDestiny.Scripts;
public partial class KillZone : Area2D
{
    public static bool IsPlayerDead { get; private set; }

    private void OnBodyEntered(Node2D body)
    {
        if (!IsPlayerDead)
        {
            IsPlayerDead = true;

            Engine.TimeScale = 0.75;

            var player = body as Player;
            player.PlayAnimation(PlayerConstants.Animations.Death);

            var timer = GetNode<Timer>(KillZoneConstants.Timer);
            timer.Start();
        }
    }

    private void OnTimerTimeout()
    {
        Engine.TimeScale = 1;
        GetTree().ReloadCurrentScene();

        IsPlayerDead = false;
    }
}
