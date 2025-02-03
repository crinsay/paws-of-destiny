using Godot;
using PawsOfDestiny.Scripts.Common;
using PawsOfDestiny.Scripts.Common.Components;
using PawsOfDestiny.Scripts.Enemies.MeowolasEnemyComponents;
using PawsOfDestiny.Scripts.Enemies.MeowtarTheBlueComponents;
using PawsOfDestiny.Scripts.Game;
using PawsOfDestiny.Singletons;
using System;

namespace PawsOfDestiny.Scripts.PlayerComponents;

public partial class Player : CharacterBody2D
{
	//Global player informations:
	public static Vector2 CurrentGlobalPosition { get; private set; }
    public PlayerState State { get; private set; } = PlayerState.Idle;
    public bool CanBeHit { get; private set; } = true;
    public int Health { get; private set; }

    //Signals:
    [Signal]
    public delegate void PlayerHitEnemyEventHandler(HitInformation hitinfo);

    //Player public variables:
    [Export]
    public float Speed = 200.0f;

	[Export]
    public float JumpVelocity = -275.0f;

    [Export]
    public float KnockbackStrength = 120.0f;

    [Export]
    public int Damage = 1;

    //Player private variables:
    private AnimatedSprite2D _animatedSprite2D;
    private Area2D _sword;
    private AnimationPlayer _animationPlayer;

    private bool _isPlayerJustHit = false;
    private HitInformation _hitInfo;
    private GameManager _gameManager;
    private PlayerStats _playerStats;

    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager");
        _gameManager.Connect(GameManager.SignalName.EnemyHitPlayer,
           new Callable(this, nameof(OnGameManagerEnemyHitPlayer)));

        _playerStats = GetNode<PlayerStats>("/root/PlayerStats");
        Health = _playerStats.Health;

        _animatedSprite2D = GetNode<AnimatedSprite2D>(PlayerConstants.Nodes.AnimatedSprite2D);
        _sword = GetNode<Area2D>(PlayerConstants.Nodes.Sword);  
        _animationPlayer = GetNode<AnimationPlayer>(PlayerConstants.Nodes.AnimationPlayer);

        State = PlayerState.Idle;
        CanBeHit = true;
    }

	public override void _PhysicsProcess(double delta)
	{
        Vector2 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

        if (_isPlayerJustHit)
        {
            velocity.X = (int)_hitInfo.KnockbackDirection * _hitInfo.KnockbackStrength;
            velocity.Y = -_hitInfo.KnockbackStrength * 1.5f;
            _isPlayerJustHit = false;
        }

        if (State == PlayerState.TakingDamage)
        {
            MovePlayer(velocity);
            return;
        }

        if (State == PlayerState.Attacking || State == PlayerState.Dead)
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
            _animatedSprite2D.Modulate = new Color(1.2f, 1.2f, 1.2f);
            GetNode<Timer>("InvincibilityAfterTakeDamageTimer").Start();
        }
        State = PlayerState.Idle;
    }

    private void OnSwordBodyEntered(Node2D body)
    {
        if (body is Node2D enemy && (enemy is MeowolasEnemy || enemy is MeowtarTheBlueEnemy ))
        {
            var hitInfo = new HitInformation
            {
                Body = body,
                Damage = Damage,
                KnockbackStrength = KnockbackStrength,
                KnockbackDirection = enemy.GlobalPosition.X < GlobalPosition.X ? Direction.Left : Direction.Right
            };

            _gameManager.OnPlayerHitEnemy(hitInfo);
        }
    }

    private void OnGameManagerEnemyHitPlayer(HitInformation hitInfo)
	{
        _hitInfo = hitInfo;

        _playerStats.Health -= _hitInfo.Damage;
        Health = _playerStats.Health;
        if (Health > 0)
        {
            _animatedSprite2D.Play(PlayerConstants.Animations.TakeDamage);
            State = PlayerState.TakingDamage;
            _isPlayerJustHit = true;
            CanBeHit = false;
        }
        else
        {
            _animatedSprite2D.Play(PlayerConstants.Animations.Death);
            State = PlayerState.Dead;
            CanBeHit = false;
        }
    }

    private void OnInvincibilityAfterTakeDamageTimerTimeout()
    {
        _animatedSprite2D.Modulate = new Color(1.0f, 1.0f, 1.0f);
        CanBeHit = true;
    }
}
