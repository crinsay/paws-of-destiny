using Godot;
using PawsOfDestiny.Scripts.Game.GameManagerComponents;
using PawsOfDestiny.Scripts.PlayerComponents;
using System;

namespace PawsOfDestiny.Scripts.Enemies.MeowolasEnemyComponents;

public partial class MeowolasEnemy : CharacterBody2D
{
	[Export]
	public PackedScene ArrowsScene { get; set; }

	private enum Direction
	{
		Left = -1,
		Right = 1
	}
	private Direction _direction = Direction.Left;

	private Timer _shootCooldownTimer;
    private RayCast2D _rightRayCast2D;
    private RayCast2D _leftRayCast2D;
    private AnimatedSprite2D _animatedSprite2D;

    public override void _Ready()
	{
		_shootCooldownTimer = GetNode<Timer>("ShootCooldownTimer");
		_shootCooldownTimer.Start();

        _rightRayCast2D = GetNode<RayCast2D>(MeowolasEnemyConstants.Nodes.RightRayCast2D);
		_leftRayCast2D = GetNode<RayCast2D>(MeowolasEnemyConstants.Nodes.LeftRayCast2D);
		_animatedSprite2D = GetNode<AnimatedSprite2D>(MeowolasEnemyConstants.Nodes.AnimatedSprite2D);

        _animatedSprite2D.FlipH = _direction == Direction.Left;
        _animatedSprite2D.Play("Attack2");
		_animatedSprite2D.Frame = 6;
    }

    public override void _Process(double delta)
	{
		if (_rightRayCast2D.IsColliding()) 
		{
            _direction = Direction.Left;
			_animatedSprite2D.FlipH = true;
		}
		else if (_leftRayCast2D.IsColliding())
		{
			_direction = Direction.Right;
            _animatedSprite2D.FlipH = false;
        }

        Vector2 position = Position;

		position.X += (int)_direction * MeowolasEnemyConstants.Movement.Speed * (float)delta;

		Position = position;
	}

    private void OnShootCooldownTimerTimeout()
	{
		var arrow = ArrowsScene.Instantiate<MeowolasArrow>();
        arrow.Direction = GlobalPosition.DirectionTo(Player.CurrentGlobalPosition);
		arrow.GlobalPosition = GlobalPosition;

        AddChild(arrow);

        _animatedSprite2D.FlipH = _direction == Direction.Left;
    }
}
