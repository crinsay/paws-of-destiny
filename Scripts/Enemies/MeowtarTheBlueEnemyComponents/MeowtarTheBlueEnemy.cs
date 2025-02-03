using Godot;
using PawsOfDestiny.Scripts.Common;
using PawsOfDestiny.Scripts.Common.Components;
using PawsOfDestiny.Scripts.Common.MeowtarTheBlueComponents;
using PawsOfDestiny.Scripts.Enemies.MeowolasEnemyComponents;
using PawsOfDestiny.Scripts.Enemies.MeowtarTheBlueEnemyComponents;
using PawsOfDestiny.Scripts.PlayerComponents;
using PawsOfDestiny.Singletons;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace PawsOfDestiny.Scripts.Enemies.MeowtarTheBlueComponents;

public partial class MeowtarTheBlueEnemy : CharacterBody2D
{

	[Export]
	public float Speed = 55.0f;

    [Export]
    public float JumpVelocity = -275.0f;

    [Export]
    public int Damage = 2;

    [Export]
    public float KnockbackStrength = 180.0f;

    [Export]
    public float MinTeleportationCoordinationX = 600.0f;
    [Export]
    public float MaxTeleportationCoordinationX = 1100.0f;
    [Export]
    public float MinTeleportationCoordinationY = -860.0f;
    [Export]
    public float MaxTeleportationCoordinationY = -900.0f;


    [Export]
    public PackedScene FireballsScene { get; set; }


    public MeowtarTheBlueState State { get; private set; } = MeowtarTheBlueState.ShootFireball;
    public bool CanBeHit { get; private set; } = true;
    public int Health { get; set; } = 9;

	private GameManager _gameManager;

	private AnimatedSprite2D _animatedSprite2D;
    private AnimationPlayer _animationPlayer;
	private HealthBar _healthBar;
    private Area2D _groundFireAttackHitbox;
	private Area2D _attackOrDodgeDecisionRange;
	private Timer _shootFireballCooldownTimer;
	private Timer _teleportationTimer;
    private Timer _startHealingTimer;
    private Timer _healingTimer;
    private Timer _deathTimer;
	private Label _stateForDebug;

	private Direction _direction = Direction.Left;
	private bool _isJustHit = false;
	private HitInformation _hitInfo;
    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager");
		_gameManager.Connect(GameManager.SignalName.PlayerHitMeowtarTheBlueEnemy,
			new Callable(this, nameof(OnPlayerHitMeowtarTheBlueEnemy)));

        _animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        _groundFireAttackHitbox = GetNode<Area2D>("GroundFireAttackHitbox");
		_attackOrDodgeDecisionRange = GetNode<Area2D>("GroundFireAttackOrDodgeRangeDecision");

		_healthBar = GetNode<HealthBar>("HealthbarCanvasLayer/HealthBar");
		_healthBar.InitializeHealthBarComponent(Health, Health);

		_shootFireballCooldownTimer = GetNode<Timer>("ShootFireballCooldownTimer");
        _teleportationTimer = GetNode<Timer>("TeleportationTimer");
        _startHealingTimer = GetNode<Timer>("StartHealingTimer");
        _healingTimer = GetNode<Timer>("HealingTimer");
        _deathTimer = GetNode<Timer>("DeathTimer");

		_stateForDebug = GetNode<Label>("StateForDebug");
		HandleShootFireball();

        _startHealingTimer.Start();
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		if (_isJustHit)
		{
			HandleKnockback(ref velocity);
        }
        else if (State == MeowtarTheBlueState.GroundFireAttack || State == MeowtarTheBlueState.Dodge)
        {
            velocity.X = 0.0f;
        }
        else if (State == MeowtarTheBlueState.ShootFireball)
        {
            velocity.X = 0.0f;
            SetDirectionTowardPlayer();
        }

		//No need for constant States Process.

		Velocity = velocity;
		MoveAndSlide();
	}


	private void HandleShootFireball()
	{
        _stateForDebug.Text = nameof(MeowtarTheBlueState.ShootFireball);
        State = MeowtarTheBlueState.ShootFireball;

        _shootFireballCooldownTimer.Start();
        SetDirectionTowardPlayer();
        PlayAnimation("Attack1");
	}

	private void HandleGroundFireAttack()
	{
        _stateForDebug.Text = nameof(MeowtarTheBlueState.GroundFireAttack);
        State = MeowtarTheBlueState.GroundFireAttack;

        SetDirectionTowardPlayer();

        //Reset because sometimes the Animation Player is not ending before next in a row GroundFireAttack and then the collision is not being enabled:

        _animationPlayer.Stop();
        _groundFireAttackHitbox.Scale = _direction == Direction.Left ? new Vector2(-1, 1) : new Vector2(1, 1);
        _animationPlayer.Play("GroundFireAttack");

        PlayAnimation("Attack2");
    }

	private void HandleDodge()
	{
        _stateForDebug.Text = nameof(MeowtarTheBlueState.Dodge);
        State = MeowtarTheBlueState.Dodge;

        //Dodge is basically a teleportation:
         _teleportationTimer.Start();
        CanBeHit = false;

        SetDirectionTowardPlayer();
        PlayAnimation("Dodge");
    }

    private void HandleTeleportation()
    {
        float newCoordinateX = (float)GD.RandRange(MinTeleportationCoordinationX, MaxTeleportationCoordinationX);
        float newCoordinateY = (float)GD.RandRange(MinTeleportationCoordinationY, MaxTeleportationCoordinationY);

        Position = new Vector2(newCoordinateX, newCoordinateY);
        CanBeHit = true;
    }

    private void HandleStartHealing()
    {
        _stateForDebug.Text = nameof(MeowtarTheBlueState.Heal);
        State = MeowtarTheBlueState.Heal;

        _healingTimer.Start();
        _shootFireballCooldownTimer.Stop();
        _startHealingTimer.Stop();

        SetDirectionTowardPlayer();
        PlayAnimation("Heal");
    }

    private void HandleHealing()
    {
        Health++;
        _healthBar.Health = Health;
    }

    private void HandleKnockback(ref Vector2 velocity)
    {
        _shootFireballCooldownTimer.Stop();

        velocity.X = (int)_hitInfo.KnockbackDirection * _hitInfo.KnockbackStrength;
        velocity.Y = -_hitInfo.KnockbackStrength * 1.5f;
        _isJustHit = false;
    }

    private void OnGroundFireAttackOrDodgeRangeDecisionBodyEntered(Node2D body)
	{
		if (body is Player)
		{
			if (GD.Randf() < 0.07f * (10 - Health))
			{
                HandleDodge();
            }
			else
			{
				HandleGroundFireAttack();
            }

            _shootFireballCooldownTimer.Stop();
            _startHealingTimer.Stop();
            _healingTimer.Stop();

            GetNode<CollisionShape2D>("GroundFireAttackOrDodgeRangeDecision/CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        }
    }
    private void OnGroundFireAttackOrDodgeRangeDecisionBodyExited(Node2D body)
    {
        if (body is Player)
        {
            _startHealingTimer.Start();
        }
    }


    private void OnAnimatedSprite2DAnimationFinished()
	{
		if (State == MeowtarTheBlueState.GroundFireAttack 
            || State == MeowtarTheBlueState.Dodge 
            || State == MeowtarTheBlueState.TakeDamage
            || State == MeowtarTheBlueState.Heal)
		{
            State = MeowtarTheBlueState.ShootFireball;
			HandleShootFireball();

            GetNode<CollisionShape2D>("GroundFireAttackOrDodgeRangeDecision/CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
            CanBeHit = true;
		}

        if (State == MeowtarTheBlueState.Heal)
        {
            if (Health < 9)
            {
                Health++;
                _healthBar.Health = Health;
            }
        }
	}

	private void PlayAnimation(StringName animationName)
	{
		_animatedSprite2D.FlipH = _direction == Direction.Left;
		_animatedSprite2D.Play(animationName);
	}

	private void OnShootFireballCooldownTimerTimeout()
	{
        var fireball = FireballsScene.Instantiate<MeowtarTheBlueFireball>();
        fireball.Direction = GlobalPosition.DirectionTo(Player.CurrentGlobalPosition);
        fireball.GlobalPosition = GlobalPosition;

        AddChild(fireball);
        _gameManager.OnMeowtarTheBlueEnemyNewFireballInstantiated(fireball);
    }

    private void OnGroundFireAttackHitboxBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            var hitInfo = new HitInformation
            {
                Body = body,
                Damage = Damage,
                KnockbackStrength = KnockbackStrength,
                KnockbackDirection = Player.CurrentGlobalPosition.X < GlobalPosition.X ? Direction.Left : Direction.Right
            };

            _gameManager.OnEnemyHitPlayer(hitInfo);
        }
    }


    private void OnPlayerHitMeowtarTheBlueEnemy(HitInformation hitInfo)
	{
        _hitInfo = hitInfo;

        Health -= _hitInfo.Damage;
        _healthBar.Health = Health;
        if (Health > 0)
        {
            _stateForDebug.Text = nameof(MeowtarTheBlueState.TakeDamage);
            State = MeowtarTheBlueState.TakeDamage;

            PlayAnimation("TakeDamage");
            _isJustHit = true;
        }
        else
        {
            _stateForDebug.Text = nameof(MeowtarTheBlueState.Death);
            State = MeowtarTheBlueState.Death;

            PlayAnimation("Death");
            _deathTimer.Start();
            GetNode<CollisionShape2D>("GroundFireAttackOrDodgeRangeDecision/CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
            GetNode<CollisionShape2D>("GroundFireAttackHitbox/GroundFireAttackCollisionShape").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        }

        _shootFireballCooldownTimer.Stop();
        _teleportationTimer.Stop();
        _startHealingTimer.Stop();
        _healingTimer.Stop();
        _animationPlayer.Stop();

        CanBeHit = false;
    }

    private void SetDirectionTowardPlayer()
    {
        if (Math.Abs(GlobalPosition.X - Player.CurrentGlobalPosition.X) > 10.0f)
        {
            if (GlobalPosition.X < Player.CurrentGlobalPosition.X)
            {
                _direction = Direction.Right;
            }
            else
            {
                _direction = Direction.Left;
            }
        }
    }

    private void OnDeathTimerTimeout()
    {
        QueueFree();
    }

    private void OnTeleportationTimerTimeout()
    {
        HandleTeleportation();
    }

    private void OnStartHealingTimerTimeout()
    {
        GD.Print("Check if need healing");
        if (Health < 9)
        {
            GD.Print("StartHealing!");

            HandleStartHealing();
        }
    }

    private void OnHealingTimerTimeout()
    {
        if (Health < 9)
        {
            HandleHealing();
            GD.Print($"Healing... {Health}");
        }
        else
        {
            _healingTimer.Stop();
            GD.Print($"Stopped healing! {Health}");
            HandleShootFireball();
        }
    }

    private void EnableGroundFireAttackCollisionShape()
    {
        GetNode<CollisionShape2D>("GroundFireAttackHitbox/GroundFireAttackCollisionShape").SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
    }

    private void DisableGroundFireAttackCollisionShape()
    {
        GetNode<CollisionShape2D>("GroundFireAttackHitbox/GroundFireAttackCollisionShape").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
    }
}
