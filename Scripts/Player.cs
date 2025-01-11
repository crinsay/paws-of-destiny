using Godot;
using PawsOfDestiny.Scripts.Constants;
using System;

namespace PawsOfDestiny.Scripts;

public partial class Player : CharacterBody2D
{
	public const float Speed = 200.0f;
	public const float JumpVelocity = -275.0f;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed(nameof(MoveDirection.MoveUp)) && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		//No Up/Down constant movement:
		Vector2 direction = Input.GetVector(nameof(MoveDirection.MoveLeft),
            nameof(MoveDirection.MoveRight), 
			nameof(MoveDirection.None),
            nameof(MoveDirection.None));

		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed * 0.2f);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
