using Godot;
using PawsOfDestiny.Scripts.Constants;
using System;

namespace PawsOfDestiny.Scripts;
public partial class KillZone : Area2D
{
    private void OnBodyEntered(Node2D body)
    {
        GD.Print("You died! :(");
        Engine.TimeScale = 0.5;
        body.GetNode("CollisionShape2D").QueueFree();

        var timer = GetNode<Timer>(KillZoneConstants.Timer);
        timer.Start();
    }

    private void OnTimerTimeout()
    {
        Engine.TimeScale = 1;
        GetTree().ReloadCurrentScene();
    }
}
