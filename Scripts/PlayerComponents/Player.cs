using Godot;
using PawsOfDestiny.Scripts.Common;
using PawsOfDestiny.Scripts.Game;
using System;

namespace PawsOfDestiny.Scripts.PlayerComponents;

public partial class Player : CharacterBody2D
{
    private AnimatedSprite2D _animatedSprite2D;

    public override void _Ready()
    {
        _animatedSprite2D = GetNode<AnimatedSprite2D>(PlayerConstants.Nodes.AnimatedSprite2D);
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
		if (Input.IsActionJustPressed(InputActions.Jump) && IsOnFloor())
		{
			velocity.Y = PlayerConstants.Movement.JumpVelocity;
		}

		if (!GameState.IsPlayerDead)
		{
			var direction = Input.GetAxis(InputActions.MoveLeft, InputActions.MoveRight);

			if (direction != 0f)
			{
				velocity.X = direction * PlayerConstants.Movement.Speed;

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
				velocity.X = Mathf.MoveToward(Velocity.X, 0, PlayerConstants.Movement.Speed * 0.2f);

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
		else
		{
			//If player is dead, stop him:
            velocity.X = Mathf.MoveToward(Velocity.X, 0, PlayerConstants.Movement.Speed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }

	public void PlayAnimation(StringName animationName)
	{
		_animatedSprite2D.Play(animationName);
	}
}
