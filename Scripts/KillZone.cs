using Godot;
using PawsOfDestiny.Scripts.Constants;
using System;

namespace PawsOfDestiny.Scripts;
public partial class KillZone : Area2D
{
    private void OnBodyEntered(Node2D body)
    {
        GD.Print("You died! :(");

        var timer = GetNode<Timer>(KillZoneConstants.Timer);
        timer.Start();
    }

    private void OnTimerTimeout()
    {
        GetTree().ReloadCurrentScene();
    }
}
