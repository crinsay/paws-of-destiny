using Godot;
using System;

namespace PawsOfDestiny.Scripts.Enemies.MeowolasEnemyComponents;

public partial class MeowolasEnemy : CharacterBody2D
{
	[Signal]
	public delegate void HitPlayerEventHandler(Node2D body);

	private enum Direction
	{
		Left = -1,
		Right = 1
	}
	private Direction _direction = Direction.Left;

    private RayCast2D _rightRayCast2D;
    private RayCast2D _leftRayCast2D;
    private AnimatedSprite2D _animatedSprite2D;

    public override void _Ready()
	{
		_rightRayCast2D = GetNode<RayCast2D>(MeowolasEnemyConstants.Nodes.RightRayCast2D);
		_leftRayCast2D = GetNode<RayCast2D>(MeowolasEnemyConstants.Nodes.LeftRayCast2D);
		_animatedSprite2D = GetNode<AnimatedSprite2D>(MeowolasEnemyConstants.Nodes.AnimatedSprite2D);
		_animatedSprite2D.Play(MeowolasEnemyConstants.Animations.Run);
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
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

	private void OnArea2DBodyEntered(Node2D body)
	{
		EmitSignal(SignalName.HitPlayer, body);
	}
}
