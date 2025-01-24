using Godot;
using PawsOfDestiny.Scripts.Game.GameManagerComponents;
using PawsOfDestiny.Scripts.PlayerComponents;
using System;

namespace PawsOfDestiny.Scripts.Enemies.MeowolasEnemyComponents;

public partial class MeowolasEnemy : CharacterBody2D
{
    public EnemyState State { get; private set; } = EnemyState.Attacking;
    public bool CanBeHit { get; private set; } = true;

    [Signal]
	public delegate void NewArrowInstantiatedEventHandler(Node2D newArrow);

    [Export]
	public PackedScene ArrowsScene { get; set; }

	[Export]
    public float Speed = 60.0f;

    private enum Direction
	{
		Left = -1,
		Right = 1
	}

	private Timer _shootCooldownTimer;
    private RayCast2D _rightRayCast2D;
    private RayCast2D _leftRayCast2D;
    private AnimatedSprite2D _animatedSprite2D;
    private Direction _moveDirection = Direction.Left;
    private Direction _knockbackDirection = Direction.Left;

    public override void _Ready()
	{
		_shootCooldownTimer = GetNode<Timer>(MeowolasEnemyConstants.Nodes.ShootCooldownTimer);
        _rightRayCast2D = GetNode<RayCast2D>(MeowolasEnemyConstants.Nodes.RightRayCast2D);
		_leftRayCast2D = GetNode<RayCast2D>(MeowolasEnemyConstants.Nodes.LeftRayCast2D);
		_animatedSprite2D = GetNode<AnimatedSprite2D>(MeowolasEnemyConstants.Nodes.AnimatedSprite2D);

        _shootCooldownTimer.Start();

        _animatedSprite2D.FlipH = _moveDirection == Direction.Left;
        _animatedSprite2D.Play(MeowolasEnemyConstants.Animations.Attack2);
		_animatedSprite2D.Frame = 6;
    }

    public override void _Process(double delta)
	{
        Vector2 velocity = Velocity;

        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        if (_rightRayCast2D.IsColliding()) 
		{
            _moveDirection = Direction.Left;
			_animatedSprite2D.FlipH = true;
		}
		else if (_leftRayCast2D.IsColliding())
		{
            _moveDirection = Direction.Right;
            _animatedSprite2D.FlipH = false;
        }

		if (State == EnemyState.JustHit)
        {
            velocity.X = (int)_knockbackDirection * 120.0f;
            velocity.Y = -200.0f;
            State = EnemyState.TakingDamage;
        }
        else if (State == EnemyState.Attacking)
        {
            _animatedSprite2D.Play(MeowolasEnemyConstants.Animations.Attack2);
            velocity.X = (int)_moveDirection * Speed;
        }

		Velocity = velocity;
		MoveAndSlide();
	}

	private void OnGameManagerPlayerHitMeowolasEnemy(int damage)
	{
		if (GlobalPosition.X < Player.CurrentGlobalPosition.X)
		{
            _knockbackDirection = Direction.Left;
        }
		else
		{
            _knockbackDirection = Direction.Right;
        }

        State = EnemyState.JustHit;
        CanBeHit = false;
        _animatedSprite2D.Play(MeowolasEnemyConstants.Animations.TakeDamage);
        _shootCooldownTimer.Stop();
    }

    private void OnShootCooldownTimerTimeout()
	{
		var arrow = ArrowsScene.Instantiate<MeowolasArrow>();
        arrow.Direction = GlobalPosition.DirectionTo(Player.CurrentGlobalPosition);
		arrow.GlobalPosition = GlobalPosition;

        AddChild(arrow);
		EmitSignal(SignalName.NewArrowInstantiated, arrow);

        _animatedSprite2D.FlipH = _moveDirection == Direction.Left;
    }

    private void OnAnimatedSprite2DAnimationFinished()
    {
        State = EnemyState.Attacking;
        CanBeHit = true;
        _animatedSprite2D.Play(MeowolasEnemyConstants.Animations.Attack2);
        _animatedSprite2D.Frame = 6;
        _shootCooldownTimer.Start();
    }
}
