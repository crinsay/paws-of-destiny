using Godot;
using PawsOfDestiny.Scripts.Common;
using PawsOfDestiny.Scripts.Common.Components;
using PawsOfDestiny.Scripts.Enemies.MeowtarTheBlueEnemyComponents;
using PawsOfDestiny.Scripts.PlayerComponents;
using PawsOfDestiny.Singletons;
using System;

namespace PawsOfDestiny.Scripts.Enemies.MeowtarTheBlueComponents;

public partial class MeowtarTheBlueEnemy : CharacterBody2D
{
	[Export]
	public float Speed = 300.0f;

    [Export]
    public float JumpVelocity = -400.0f;


    public MeowtarTheBlueState State { get; private set; } = MeowtarTheBlueState.ShootFireball;
    public bool CanBeHit { get; private set; } = true;
    public int Health { get; set; } = 9;

	private AnimatedSprite2D _animatedSprite2D;
	private HealthBar _healthBar;
	private Area2D _attackOrDodgeDecisionRange;
	private Label _stateForDebug;

	private Direction _direction = Direction.Left;
    public override void _Ready()
    {
		_animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_attackOrDodgeDecisionRange = GetNode<Area2D>("GroundFireAttackOrDodgeRangeDecision");

		_healthBar = GetNode<HealthBar>("HealthbarCanvasLayer/HealthBar");
		_healthBar.InitializeHealthBarComponent(Health, Health);

		_stateForDebug = GetNode<Label>("StateForDebug");
		_stateForDebug.Text = nameof(MeowtarTheBlueState.ShootFireball);
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private void OnGroundFireAttackOrDodgeRangeDecisionBodyEntered(Node2D body)
	{
		//if (body is Player)
		{
			if (GD.Randf() < 0.35f)
			{
                _stateForDebug.Text = nameof(MeowtarTheBlueState.Dodge);
                State = MeowtarTheBlueState.Dodge;
				PlayAnimation("Dodge");
			}
			else
			{
                _stateForDebug.Text = nameof(MeowtarTheBlueState.GroundFireAttack);
                State = MeowtarTheBlueState.GroundFireAttack;
                PlayAnimation("Attack2");
            }
		}
	}

	private void OnGroundFireAttackOrDodgeRangeDecisionBodyExited(Node2D body)
	{
		if (body is Player)
		{
			//
		}
	}

	private void OnAnimatedSprite2DAnimationFinished()
	{
		if (State == MeowtarTheBlueState.GroundFireAttack || State == MeowtarTheBlueState.Dodge)
		{
			bool _isPlayerInAttackOrDodgeDecisionRange = false;

            foreach (var body in _attackOrDodgeDecisionRange.GetOverlappingBodies())
			{
                if (body is Player)
                {
					_isPlayerInAttackOrDodgeDecisionRange = true;
					break;
                }
            }	
			
			if (_isPlayerInAttackOrDodgeDecisionRange)
			{
                //HandleDecisionAgain
                OnGroundFireAttackOrDodgeRangeDecisionBodyEntered(null!);
            }
			else
			{
                _stateForDebug.Text = nameof(MeowtarTheBlueState.ShootFireball);
                State = MeowtarTheBlueState.ShootFireball;
				PlayAnimation("Attack1");
			}
		}
	}

	private void PlayAnimation(StringName animationName)
	{
		_animatedSprite2D.FlipH = _direction == Direction.Left;
		_animatedSprite2D.Play(animationName);
	}
}
