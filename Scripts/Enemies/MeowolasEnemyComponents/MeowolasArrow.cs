using Godot;
using PawsOfDestiny.Scripts.Game.GameManagerComponents;
using PawsOfDestiny.Scripts.PlayerComponents;
using System;

namespace PawsOfDestiny.Scripts.Enemies.MeowolasEnemyComponents;

public partial class MeowolasArrow : Node2D
{ 
	[Export]
	public float Speed = 360.0f;

    [Export]
    public int Damage = 1;

    public Vector2 Direction = Vector2.Zero;

	private AnimatedSprite2D _animatedSprite2D;
	private GameManager _gameManager;

	public override void _Ready()
	{
		_animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_gameManager = GetNode<GameManager>("/root/World/GameManager");

        Rotation = Direction.Angle();
    }

    public override void _Process(double delta)
	{
		Position += Direction * Speed * (float)delta;
    }

	private void OnArrowHitBoxBodyEntered(Node2D body)
	{
        if (body is not MeowolasEnemy)
		{
			if (body is Player)
			{
				_gameManager.OnMeowolasArrowHitPlayer(Damage);
			}

            QueueFree();
        }
    }
}
