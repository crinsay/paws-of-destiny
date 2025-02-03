using Godot;
using PawsOfDestiny.Scripts.Common;
using PawsOfDestiny.Scripts.PlayerComponents;
using PawsOfDestiny.Singletons;
using System;
using static System.Net.Mime.MediaTypeNames;

public partial class BallSpikes : PathFollow2D
{
    [Export]
    public float Speed = 150f;
    [Export]
    public float SpinSpeed = 400f;


    private int Damage = 1;
    private GameManager _gameManager;
    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager");
    }
    public override void _Process(double delta)
    {
        Progress += Speed * (float)delta;
        RotationDegrees += (float)(delta * SpinSpeed);
    }

    private void OnBallSpikesBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            var hitInfo = new HitInformation
            {
                Body = body,
                Damage = Damage
            };

            _gameManager.OnSpikesHitPlayer(hitInfo);
        }
    }
}
