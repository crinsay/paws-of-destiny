using Godot;
using PawsOfDestiny.Scripts.Common;
using PawsOfDestiny.Scripts.PlayerComponents;
using System;

namespace PawsOfDestiny.Scripts.KingMeowthurComponents;

public partial class KingMeowthur : CharacterBody2D
{
	public const float Speed = 110.0f;
	public const float JumpVelocity = -175.0f;

    private AnimatedSprite2D _animatedSprite2D;
    private Timer _startMovingTimer;
    private Direction _moveDirection;
    private bool _isMoving = false;

    public override void _Ready()
    {
        _animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _startMovingTimer = GetNode<Timer>("StartMovingTimer");
    }

    public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}
	
        if (ShouldMove() && _isMoving)
        {
            velocity.X = (int)_moveDirection * Speed;

            SetDirectionTowardPlayer();
            PlayAnimation("Run");
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed * 0.2f);
            _isMoving = false;

            PlayAnimation("Idle");
        }

		Velocity = velocity;
		MoveAndSlide();
	}

    private bool ShouldMove()
    {
        if (Math.Abs(GlobalPosition.X - Player.CurrentGlobalPosition.X) > 50.0f)
        {
            if (!_isMoving && _startMovingTimer.IsStopped())
            {
                _startMovingTimer.Start();
            }

            return true;
        }

        return false;
    }

    private void SetDirectionTowardPlayer()
    {
        if (Math.Abs(GlobalPosition.X - Player.CurrentGlobalPosition.X) > 10.0f)
        {
            if (GlobalPosition.X < Player.CurrentGlobalPosition.X)
            {
                _moveDirection = Direction.Right;
            }
            else
            {
                _moveDirection = Direction.Left;
            }
        }
    }

    private void PlayAnimation(StringName animationName)
    {
        _animatedSprite2D.FlipH = _moveDirection == Direction.Left;
        _animatedSprite2D.Play(animationName);
    }

    private void OnStartMovingTimerTimeout()
    {
        _isMoving = true;
    }
}
