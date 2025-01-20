using Godot;
using PawsOfDestiny.Scripts.Constants;
using System;

namespace PawsOfDestiny.Scripts;

public partial class Player : CharacterBody2D
{
	public const float Speed = 200.0f;
	public const float JumpVelocity = -275.0f;

    private AnimatedSprite2D _animatedSprite2D;
	private KillZone _killZone;

    public override void _Ready()
    {
        _animatedSprite2D = GetNode<AnimatedSprite2D>(PlayerConstants.Nodes.AnimatedSprite2D);
		_killZone = GetNode<KillZone>("%KillZone");
    }

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed(PlayerConstants.InputActions.Jump) && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		if (!KillZone.IsPlayerDead)
		{
			var direction = Input.GetAxis(PlayerConstants.InputActions.MoveLeft,
		 PlayerConstants.InputActions.MoveRight);

			if (direction != 0f)
			{
				velocity.X = direction * Speed;

				_animatedSprite2D.Play(PlayerConstants.Animations.Run);
				if (direction > 0f)
				{
					_animatedSprite2D.FlipH = false;
				}
				else if (direction < 0f)
				{
					_animatedSprite2D.FlipH = true;
				}
			}
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed * 0.2f);

				if (IsOnFloor())
				{
					_animatedSprite2D.Play(PlayerConstants.Animations.Idle);
				}
				else
				{
					_animatedSprite2D.Play(PlayerConstants.Animations.Jump);
				}
			}
		}
        Velocity = velocity;
        MoveAndSlide();
    }

	public void PlayAnimation(StringName animationName)
	{
		_animatedSprite2D.Play(animationName);
	}
}
