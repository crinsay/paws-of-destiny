using Godot;
using PawsOfDestiny.Scripts.Common;
using PawsOfDestiny.Scripts.Common.Components;
using PawsOfDestiny.Scripts.Game.GameManagerComponents;
using PawsOfDestiny.Scripts.PlayerComponents;
using System;

namespace PawsOfDestiny.Scripts.Enemies.MeowolasEnemyComponents;

public partial class MeowolasEnemy : CharacterBody2D
{
    public EnemyState State { get; private set; } = EnemyState.Patrol;
    public bool CanBeHit { get; private set; } = true;
    public int Health { get; private set; } = 9;

    [Signal]
    public delegate void NewArrowInstantiatedEventHandler(Node2D newArrow);

    [Signal]
    public delegate void EnemyHitPlayerEventHandler(HitInformation hitInfo);

    [Export]
    public PackedScene ArrowsScene { get; set; }

    [Export]
    public float Speed = 55.0f;

    [Export]
    public int Damage = 1;

    [Export]
    public float KnockbackStrength = 120.0f;

    private Timer _shootCooldownTimer;
    private AnimationPlayer _animationPlayer;
    private Area2D _kick;
    private RayCast2D _rightRayCast2D;
    private RayCast2D _leftRayCast2D;
    private AnimatedSprite2D _animatedSprite2D;
    private HealthBar _healthBar;
    private Timer _deathTimer;
    private Timer _knockbackTimer;
    private Timer _changeStateTimer;
    private Direction _moveDirection = Direction.Left;
    private Direction _knockbackDirection;
    private EnemyState _awaitingState;

    private bool _isEnemyJustHit = false;
    private Label _stateForDebug;

    public override void _Ready()
    {
        _shootCooldownTimer = GetNode<Timer>(MeowolasEnemyConstants.Nodes.ShootCooldownTimer);
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _kick = GetNode<Area2D>("Kick");
        _rightRayCast2D = GetNode<RayCast2D>(MeowolasEnemyConstants.Nodes.RightRayCast2D);
        _leftRayCast2D = GetNode<RayCast2D>(MeowolasEnemyConstants.Nodes.LeftRayCast2D);
        _animatedSprite2D = GetNode<AnimatedSprite2D>(MeowolasEnemyConstants.Nodes.AnimatedSprite2D);
        _healthBar = GetNode<HealthBar>(MeowolasEnemyConstants.Nodes.HealthBar);
        _deathTimer = GetNode<Timer>(MeowolasEnemyConstants.Nodes.DeathTimer);
        _knockbackTimer = GetNode<Timer>("KnockbackTimer");
        _changeStateTimer = GetNode<Timer>("ChangeStateTimer");
        _stateForDebug = GetNode<Label>("StateForDebug");

        _animatedSprite2D.FlipH = _moveDirection == Direction.Left;
        _animatedSprite2D.Play(MeowolasEnemyConstants.Animations.Run);

        _healthBar.InitializeHealthBarComponent(Health);
    }

    public override void _Process(double delta)
    {
        Vector2 velocity = Velocity;

        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        if (_isEnemyJustHit)
        {
            velocity.X = (int)_knockbackDirection * 120.0f;
            velocity.Y = -200.0f;
            _isEnemyJustHit = false;
        }
        else if (CanBeHit)
        {
            if (State == EnemyState.Death)
            {
                _stateForDebug.Text = nameof(EnemyState.Death);
                velocity.X = Mathf.MoveToward(Velocity.X, 0f, Speed);
            }
            else if (State == EnemyState.Patrol)
            {
                _animatedSprite2D.FlipH = _moveDirection == Direction.Left;
                _stateForDebug.Text = nameof(EnemyState.Patrol);
                velocity.X = (int)_moveDirection * Speed;
                _animatedSprite2D.Play(MeowolasEnemyConstants.Animations.Run);

                if (_rightRayCast2D.IsColliding())
                {
                    _moveDirection = Direction.Left;
                }
                else if (_leftRayCast2D.IsColliding())
                {
                    _moveDirection = Direction.Right;
                }
            }
            else if (State == EnemyState.Chase)
            {
                _stateForDebug.Text = nameof(EnemyState.Chase);
                SetDirectionTowardPlayer();
                velocity.X = (int)_moveDirection * Speed;
                _animatedSprite2D.Play(MeowolasEnemyConstants.Animations.Run);                

                if (_rightRayCast2D.IsColliding()
               && velocity.X > 0
               && IsOnFloor())
                {
                    velocity.Y = -275.0f;
                }
                else if (_leftRayCast2D.IsColliding()
                         && velocity.X < 0
                         && IsOnFloor())
                {
                    velocity.Y = -275.0f;
                }
            }
            else if (State == EnemyState.Shoot)
            {
                _stateForDebug.Text = nameof(EnemyState.Shoot);
                SetDirectionTowardPlayer();
                velocity.X = 0.0f;
                _animatedSprite2D.Play(MeowolasEnemyConstants.Animations.Attack2);
            }
            else if (State == EnemyState.Kick)
            {
                _stateForDebug.Text = nameof(EnemyState.Kick);
                SetDirectionTowardPlayer();
                velocity.X = 0.0f;
            }
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

        Health -= damage;
        _healthBar.Health = Health;
        _shootCooldownTimer.Stop();

        if (Health > 0)
        {
            _animatedSprite2D.Play(MeowolasEnemyConstants.Animations.TakeDamage);
            _knockbackTimer.Start();
            _isEnemyJustHit = true;
        }
        else
        {
            _stateForDebug.Text = nameof(EnemyState.Death);
            _animatedSprite2D.FlipH = _moveDirection == Direction.Left;
            _animatedSprite2D.Play(MeowolasEnemyConstants.Animations.Death);
            State = EnemyState.Death;
            _deathTimer.Start();
            GetNode<CollisionShape2D>("SightRange/CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
            GetNode<CollisionShape2D>("ShootRange/CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
            GetNode<CollisionShape2D>("KickRange/CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        }

        CanBeHit = false;
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

    private void OnDeathTimerTimeout()
    {
        QueueFree();
    }

    private void OnKnockbackTimerTimeout()
    {
        ContinueCurrentState();
        CanBeHit = true;
    }

    private void OnSightRangeBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            State = EnemyState.Chase;
            Speed = 110.0f;
            _animatedSprite2D.Play(MeowolasEnemyConstants.Animations.Run);
        }
    }

    private void OnSightRangeBodyExited(Node2D body)
    {
        if (body is Player && Health > 0)
        {
            State = EnemyState.Patrol;
            Speed = 55.0f;
            _animatedSprite2D.Play(MeowolasEnemyConstants.Animations.Run);
        }
    }

    private void OnShootRangeBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            _awaitingState = EnemyState.Shoot;
            _changeStateTimer.WaitTime = 0.1f;
            _changeStateTimer.Start();
        }
    }

    private void OnShootRangeBodyExited(Node2D body)
    {
        if (body is Player && Health > 0)
        {
            Speed = 110.0f;
            State = EnemyState.Chase;
            _shootCooldownTimer.Stop();
            _animatedSprite2D.Play(MeowolasEnemyConstants.Animations.Run);
        }
    }

    private void OnKickRangeBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            State = EnemyState.Kick;
            _animatedSprite2D.FlipH = _moveDirection == Direction.Left;
            _kick.Scale = _moveDirection == Direction.Left ? new Vector2(-1, 1) : new Vector2(1, 1);
            _animatedSprite2D.Play(MeowolasEnemyConstants.Animations.Attack3);
            _shootCooldownTimer.Stop();
            _animationPlayer.Play("Kick");
        }
    }

    private void SetDirectionTowardPlayer()
    {
        if (Math.Abs(GlobalPosition.X - Player.CurrentGlobalPosition.X) > 10.0f)
        {
            if (GlobalPosition.X < Player.CurrentGlobalPosition.X)
            {
                _moveDirection = Direction.Right;
                _animatedSprite2D.FlipH = false;
            }
            else
            {
                _moveDirection = Direction.Left;
                _animatedSprite2D.FlipH = true;
            }
        }
    }

    private void ContinueCurrentState()
    {
        if (State == EnemyState.Shoot)
        {
            _shootCooldownTimer.Start();
            _animatedSprite2D.Play(MeowolasEnemyConstants.Animations.Attack2);
            _animatedSprite2D.Frame = 6;
            _animatedSprite2D.FlipH = _moveDirection == Direction.Left;
        }
    }

    private void OnKickBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            var hitInfo = new HitInformation
            {
                Damage = Damage,
                KnockbackStrength = KnockbackStrength,
                KnockbackDirection = Player.CurrentGlobalPosition.X < GlobalPosition.X ? Common.Direction.Left : Common.Direction.Right
            };

            EmitSignal(SignalName.EnemyHitPlayer, hitInfo);
        }
    }

    private void EnableKickCollision()
    {
        GetNode<CollisionShape2D>("Kick/KickHitbox").SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
    }

    private void DisableKickCollision()
    {
        GetNode<CollisionShape2D>("Kick/KickHitbox").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
    }

    private void OnAnimatedSprite2DAnimationFinished()
    {
        if (State == EnemyState.Kick)
        {
            State = EnemyState.Shoot;
            _shootCooldownTimer.Start();
            _animatedSprite2D.Frame = 6;
            _animatedSprite2D.Play(MeowolasEnemyConstants.Animations.Attack2); 
        }
    }

    private void OnChangeStateTimerTimeout()
    {
        if (_awaitingState == EnemyState.Shoot)
        {
            State = _awaitingState;
            _shootCooldownTimer.Start();
            _animatedSprite2D.Play(MeowolasEnemyConstants.Animations.Attack2);
            _animatedSprite2D.Frame = 6;
            _animatedSprite2D.FlipH = _moveDirection == Direction.Left;
        }
    }
}
