using Godot;
using System;

namespace PawsOfDestiny.Scripts;
public partial class KillZone : Area2D
{
    private void OnBodyEntered(Node2D body)
    {
        GD.Print("You died! :(");

        var timer = GetNode<Timer>("Timer");
        timer.Start();
    }

    private void OnTimerTimeout()
    {
        GetTree().ReloadCurrentScene();
    }
}
