using Godot;
using PawsOfDestiny.Scripts.Common;
using PawsOfDestiny.Scripts.Common.Components;
using PawsOfDestiny.Scripts.Enemies.MeowolasEnemyComponents;
using PawsOfDestiny.Scripts.Game;
using System;

namespace PawsOfDestiny.Scripts.PlayerComponents;

public partial class Player : CharacterBody2D
{
	//Global player informations:
	public static Vector2 CurrentGlobalPosition { get; private set; }
    public static PlayerState State { get; private set; } = PlayerState.Idle;
    public static bool CanBeHit { get; private set; } = true;
    public static int Health { get; private set; } = 9;

    //Signals:
    [Signal]
    public delegate void PlayerHitEnemyEventHandler(Node2D enemy, int damage);

    //Player public variables:
    [Export]
    public float Speed = 200.0f;

	[Export]
    public float JumpVelocity = -275.0f;

    [Export]
    public int Damage = 1;

    //Player private variables:
    private AnimatedSprite2D _animatedSprite2D;
    private Area2D _sword;
    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        _animatedSprite2D = GetNode<AnimatedSprite2D>(PlayerConstants.Nodes.AnimatedSprite2D);
        _sword = GetNode<Area2D>(PlayerConstants.Nodes.Sword);  
        _animationPlayer = GetNode<AnimationPlayer>(PlayerConstants.Nodes.AnimationPlayer);

        State = PlayerState.Idle;
        Health = 9;
        CanBeHit = true;
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
                velocity.X = Mathf.MoveToward(Velocity.X, 0f, Speed);
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
                _sword.Scale = new Vector2(1, 1); //Flip the hitbox to the right.
            }
            else
            {
                _animatedSprite2D.FlipH = true;
                _sword.Scale = new Vector2(-1, 1); //Flip the hitbox to the left.
            }
            _animationPlayer.Play(PlayerConstants.Animations.BasicAttack);
            _animatedSprite2D.Play(PlayerConstants.Animations.Attack2);
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
                State = PlayerState.Idle;
            }
			else
			{
                _animatedSprite2D.Play(PlayerConstants.Animations.Jump);
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

	private void OnAnimatedSprite2DAnimationFinished()
	{
        if (State == PlayerState.TakingDamage)
        {
            CanBeHit = true;
        }

        State = PlayerState.Idle;
    }

    private void OnSwordBodyEntered(Node2D body)
    {
        if (body is MeowolasEnemy enemy)
        {
            EmitSignal(SignalName.PlayerHitEnemy, enemy, Damage);
        }
    }

    private void OnGameManagerEnemyHitPlayer(int damage)
	{
        Health -= damage;
        if (Health > 0)
        {
            _animatedSprite2D.Play(PlayerConstants.Animations.TakeDamage);
            State = PlayerState.TakingDamage;
            CanBeHit = false;
        }
        else
        {
            _animatedSprite2D.Play(PlayerConstants.Animations.Death);
            State = PlayerState.Dead;
            CanBeHit = false;
        }
    }
}
