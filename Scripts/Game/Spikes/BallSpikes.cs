using Godot;
using System;

public partial class BallSpikes : PathFollow2D
{
    [Export]
    public float Speed = 0.04f;
    [Export]
    public float SpinSpeed = 400f;

    public override void _Process(double delta)
    {
        ProgressRatio += (float)(delta * Speed);
        RotationDegrees += (float)(delta * SpinSpeed);
    }
}
