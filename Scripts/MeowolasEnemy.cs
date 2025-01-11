using Godot;
using PawsOfDestiny.Scripts.Constants;
using System;

namespace PawsOfDestiny.Scripts;

public partial class MeowolasEnemy : Node2D
{
    public const float Speed = 40.0f;

	private float _direction = 1;

	RayCast2D _rightRayCast2D;
	RayCast2D _leftRayCast2D;
	AnimatedSprite2D _animatedSprite2D;

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
			_direction = -1;
			_animatedSprite2D.FlipH = true;
		}
		else if (_leftRayCast2D.IsColliding())
		{
			_direction = 1;
            _animatedSprite2D.FlipH = false;
        }

		Vector2 position = Position;

		position.X += _direction * Speed * (float)delta;

		Position = position;
	}
}
