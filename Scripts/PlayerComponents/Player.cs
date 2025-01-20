using Godot;
using PawsOfDestiny.Scripts.Common;
using PawsOfDestiny.Scripts.Game;
using System;

namespace PawsOfDestiny.Scripts.PlayerComponents;

public partial class Player : CharacterBody2D
{
	[Signal]
	public delegate void HitEnemyEventHandler(Node2D body);

    private AnimatedSprite2D _animatedSprite2D;
	private bool _isOneShotAnimationPlaying;

	public int Health { get; private set; } = 9;

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
		
		if (!GameState.IsPlayerDead)
		{

            if (Input.IsActionJustPressed(InputActions.Jump) && IsOnFloor())
            {
                velocity.Y = PlayerConstants.Movement.JumpVelocity;
            }

            var direction = Input.GetAxis(InputActions.MoveLeft, InputActions.MoveRight);

			if (direction != 0f)
			{
				velocity.X = direction * PlayerConstants.Movement.Speed;

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
				velocity.X = Mathf.MoveToward(Velocity.X, 0, PlayerConstants.Movement.Speed * 0.2f);

				if (IsOnFloor())
				{
                    PlayAnimation(PlayerConstants.Animations.Idle);
                }
				else
				{
                    PlayAnimation(PlayerConstants.Animations.Jump);
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
		if (!_isOneShotAnimationPlaying)
		{
            _animatedSprite2D.Play(animationName);
        }
	}

	private void OnAnimatedSprite2DAnimationFinished()
	{
		_isOneShotAnimationPlaying = false;
	}

    private void OnMeowolasEnemyHitPlayer(Node2D body) //The enemy which hit the player is the body here.
	{
		if (!GameState.WasPlayerHit)
		{
            if (--Health > 0)
            {
                PlayAnimation(PlayerConstants.Animations.TakeDamage);
                _isOneShotAnimationPlaying = true;
            }
            else
            {
                PlayAnimation(PlayerConstants.Animations.Death);
                _isOneShotAnimationPlaying = true;
            }

			EmitSignal(SignalName.HitEnemy, this);
        }		
	}
}
