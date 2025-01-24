using Godot;
using PawsOfDestiny.Scripts.Common;
using PawsOfDestiny.Scripts.Enemies.MeowolasEnemyComponents;
using PawsOfDestiny.Scripts.Game;
using System;

namespace PawsOfDestiny.Scripts.PlayerComponents;

public partial class Player : CharacterBody2D
{
	//Global player informations:
	public static Vector2 CurrentGlobalPosition { get; private set; }
    public static PlayerState State { get; private set; } = PlayerState.Idle;
    public static int CurrentHealth { get; private set; } = 9;


    //Player public variables:
    [Export]
    public float Speed = 200.0f;

	[Export]
    public float JumpVelocity = -275.0f;

    //Player private variables:
    private AnimatedSprite2D _animatedSprite2D;

    public override void _Ready()
    {
        _animatedSprite2D = GetNode<AnimatedSprite2D>(PlayerConstants.Nodes.AnimatedSprite2D);

        State = PlayerState.Idle;
        CurrentHealth = 9;
    }


	public override void _PhysicsProcess(double delta)
	{
        Vector2 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

        if (State == PlayerState.Attacking
            || State == PlayerState.TakingDamage
            || State == PlayerState.Dead)
        {
            if (velocity.X != 0f)
            {
                velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            }

            MovePlayer(velocity);
            return;
        }

        if (Input.IsActionJustPressed(InputActions.BasicAttack) && IsOnFloor())
        {
            var currentMousePosition = GetGlobalMousePosition();

            if (currentMousePosition.X > GlobalPosition.X)
            {
                _animatedSprite2D.FlipH = false;
            }
            else
            {
                _animatedSprite2D.FlipH = true;
            }
            PlayAnimation(PlayerConstants.Animations.Attack2);
            State = PlayerState.Attacking;

            return;
        }

        if (Input.IsActionJustPressed(InputActions.Jump) && IsOnFloor())
        {
			velocity.Y = JumpVelocity;
            State = PlayerState.Jumping;
        }

        var direction = Input.GetAxis(InputActions.MoveLeft, InputActions.MoveRight);

		if (direction != 0f)
		{
			velocity.X = direction * Speed;

            PlayAnimation(PlayerConstants.Animations.Run);
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
                PlayAnimation(PlayerConstants.Animations.Idle);
                State = PlayerState.Idle;
            }
			else
			{
                PlayAnimation(PlayerConstants.Animations.Jump);
                State = PlayerState.Running;
            }  
		}

        MovePlayer(velocity);
    }

    private void MovePlayer(Vector2 velocity)
    {
        Velocity = velocity;
        CurrentGlobalPosition = GlobalPosition;
        MoveAndSlide();
    }

	private void PlayAnimation(StringName animationName)
	{
        _animatedSprite2D.Play(animationName);
	}

	private void OnAnimatedSprite2DAnimationFinished()
	{
        State = PlayerState.Idle;
    }

    private void OnGameManagerMeowolasArrowHitPlayer(int damage)
	{
        if (!GameState.WasPlayerHit)
        {
            CurrentHealth -= damage;
            if (CurrentHealth > 0)
            {
                PlayAnimation(PlayerConstants.Animations.TakeDamage);
                State = PlayerState.TakingDamage;
            }
            else
            {
                PlayAnimation(PlayerConstants.Animations.Death);
                State = PlayerState.Dead;
            }
        }
    }
}
