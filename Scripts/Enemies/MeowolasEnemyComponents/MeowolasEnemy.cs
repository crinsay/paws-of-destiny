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
    public float Speed = 50.0f;

    [Export]
    public float JumpVelocity = -275.0f;


    private GameManagerSingleton _gameManagerSingleton;

    [Export]
    public int Damage = 1;

    [Export]
    public float KnockbackStrength = 120.0f;

    private Timer _shootCooldownTimer;
    private Timer _kickCooldownTimer;
    private AnimationPlayer _animationPlayer;
    private Area2D _kick;
    private RayCast2D _rightRayCast2D;
    private RayCast2D _leftRayCast2D;
    private AnimatedSprite2D _animatedSprite2D;
    private HealthBar _healthBar;
    private Timer _deathTimer;
    private Timer _changeStateTimer;
    private Direction _moveDirection = Direction.Left;
    private EnemyState _awaitingState;
    private HitInformation _hitInfo;

    private bool _isEnemyJustHit = false;
    private bool _wasPlayerKickedOutOfKickRange = false;
    private Label _stateForDebug;

    public override void _Ready()
	{
        _gameManagerSingleton = GetNode<GameManagerSingleton>("/root/GameManagerSingleton");
        _shootCooldownTimer = GetNode<Timer>(MeowolasEnemyConstants.Nodes.ShootCooldownTimer);
        _kickCooldownTimer = GetNode<Timer>("KickCooldownTimer");
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _kick = GetNode<Area2D>("Kick");
        _rightRayCast2D = GetNode<RayCast2D>(MeowolasEnemyConstants.Nodes.RightRayCast2D);
        _leftRayCast2D = GetNode<RayCast2D>(MeowolasEnemyConstants.Nodes.LeftRayCast2D);
        _animatedSprite2D = GetNode<AnimatedSprite2D>(MeowolasEnemyConstants.Nodes.AnimatedSprite2D);
        _healthBar = GetNode<HealthBar>(MeowolasEnemyConstants.Nodes.HealthBar);
        _deathTimer = GetNode<Timer>(MeowolasEnemyConstants.Nodes.DeathTimer);
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
            HandleKnockback(ref velocity);
        }
        else if (State != EnemyState.Kick && State != EnemyState.TakeDamage && State != EnemyState.Dodge)
        {
            switch(State)
            {
                case EnemyState.Patrol:
                    HandlePatrol(ref velocity);
                    break;
                case EnemyState.Chase:
                    HandleChase(ref velocity);
                    break;
                case EnemyState.Shoot:
                    HandleShoot(ref velocity);
                    break;
                case EnemyState.WantToAttack:
                    HandleWantToAttack(ref velocity);
                    break;
                case EnemyState.Death:
                    HandleDeath(ref velocity);
                    break;
                default:
                    break;
            }
        }
        
        Velocity = velocity;
        MoveAndSlide();
    }

    //Handle every behavior:
    private void HandlePatrol(ref Vector2 velocity)
    {
        _stateForDebug.Text = nameof(EnemyState.Patrol);
        velocity.X = (int)_moveDirection * Speed;

        //RayCast2D collision detection:
        if (_rightRayCast2D.IsColliding())
        {
            _moveDirection = Direction.Left;
        }
        else if (_leftRayCast2D.IsColliding())
        {
            _moveDirection = Direction.Right;
        }

        //Animation:
        PlayAnimation(MeowolasEnemyConstants.Animations.Run);
    }

    private void HandleChase(ref Vector2 velocity)
    {
        _stateForDebug.Text = nameof(EnemyState.Chase);
        velocity.X = (int)_moveDirection * Speed;

        //RayCast2D collision detection:
        if (_rightRayCast2D.IsColliding() && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }
        else if (_leftRayCast2D.IsColliding() && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        //Animation and direction:
        SetDirectionTowardPlayer();
        PlayAnimation(MeowolasEnemyConstants.Animations.Run);
    }

    private void HandleShoot(ref Vector2 velocity)
    {
        _stateForDebug.Text = nameof(EnemyState.Shoot);
        velocity.X = 0.0f;

        //Animation and direction:
        SetDirectionTowardPlayer();
        PlayAnimation(MeowolasEnemyConstants.Animations.Attack2);
    }

    private void HandleWantToAttack(ref Vector2 velocity)
    {
        _stateForDebug.Text = nameof(EnemyState.WantToAttack);
        velocity.X = (int)_moveDirection * Speed;

        if (_rightRayCast2D.IsColliding() && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }
        else if (_leftRayCast2D.IsColliding() && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        //Animation and direction:
        SetDirectionTowardPlayer();
        PlayAnimation(MeowolasEnemyConstants.Animations.Run);
    }

    private void HandleKick()
    {
        _stateForDebug.Text = nameof(EnemyState.Kick);

        Vector2 velocity = Velocity;
        velocity.X = 0.0f;
        Velocity = velocity;

        //Animation and direction:
        SetDirectionTowardPlayer();
        _kick.Scale = _moveDirection == Direction.Left ? new Vector2(-1, 1) : new Vector2(1, 1);
        _animationPlayer.Play("Kick");
        PlayAnimation(MeowolasEnemyConstants.Animations.Attack3);
    }

    private void HandleKnockback(ref Vector2 velocity)
    {
        velocity.X = (int)_hitInfo.KnockbackDirection * _hitInfo.KnockbackStrength;
        velocity.Y = -_hitInfo.KnockbackStrength * 1.5f;
        _isEnemyJustHit = false;
    }

    private void HandleDeath(ref Vector2 velocity)
    {
        _stateForDebug.Text = nameof(EnemyState.Death);
        velocity.X = Mathf.MoveToward(Velocity.X, 0f, Speed);
    }


    //Enemy got hit by the Player (because basically only Player can hit him):
    private void OnGameManagerPlayerHitMeowolasEnemy(HitInformation hitInfo)
    {
        _hitInfo = hitInfo;

        Health -= _hitInfo.Damage;
        _healthBar.Health = Health;
        if (Health > 0)
        {
            State = EnemyState.TakeDamage;
            PlayAnimation(MeowolasEnemyConstants.Animations.TakeDamage);
            _isEnemyJustHit = true;
        }
        else
        {
            _stateForDebug.Text = nameof(EnemyState.Death);         
            State = EnemyState.Death;
            PlayAnimation(MeowolasEnemyConstants.Animations.Death);
            _deathTimer.Start();
            GetNode<CollisionShape2D>("SightRange/CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
            GetNode<CollisionShape2D>("ShootRange/CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
            GetNode<CollisionShape2D>("WantToAttackRange/CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
            GetNode<CollisionShape2D>("AttackDecisionRange/CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
            GetNode<CollisionShape2D>("KickRange/CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
            GetNode<CollisionShape2D>("Kick/KickHitbox").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        }

        _animationPlayer.Stop();
        _shootCooldownTimer.Stop();
        _kickCooldownTimer.Stop();
        _changeStateTimer.Stop();

        CanBeHit = false;
    }


    //Just for readability:
    private void PlayAnimation(StringName animationName)
    {
        _animatedSprite2D.FlipH = _moveDirection == Direction.Left;
        _animatedSprite2D.Play(animationName);
    }

    //When in Shoot state, shoot an arrow (set to every one second):
    private void OnShootCooldownTimerTimeout()
    {
        var arrow = ArrowsScene.Instantiate<MeowolasArrow>();
        arrow.Direction = GlobalPosition.DirectionTo(Player.CurrentGlobalPosition);
        arrow.GlobalPosition = GlobalPosition;

        AddChild(arrow);
        //_gameManagerSingleton.OnMeowolasEnemyNewArrowInstantiated(arrow);
        EmitSignal(SignalName.NewArrowInstantiated, arrow);
    }

    //Enemy died, so queue free:
    private void OnDeathTimerTimeout()
    {
        QueueFree();
    }

    //Player entered sight range, so start chasing:
    private void OnSightRangeBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            State = EnemyState.Chase;
            Speed = 100.0f;
        }
    }

    //Player lost from sight, so start patroling again:
    private void OnSightRangeBodyExited(Node2D body)
    {
        if (body is Player && State != EnemyState.Death)
        {
            State = EnemyState.Patrol;
            Speed = 50.0f;
        }
    }

    //Start Shoot (after little delay):
    private void OnShootRangeBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            _awaitingState = EnemyState.Shoot;
            _changeStateTimer.Start();
        }
    }

    //Stop shooting and start chasing again:
    private void OnShootRangeBodyExited(Node2D body)
    {
        if (body is Player && State != EnemyState.Death)
        {
            Speed = 100.0f;
            State = EnemyState.Chase;

            //Stop shooting:
            _shootCooldownTimer.Stop();
            _changeStateTimer.Stop();
        }
    }

    private void OnWantToAttackRangeBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            State = EnemyState.WantToAttack;
            Speed = 150.0f;

            //Stop shooting:
            _shootCooldownTimer.Stop();
            _changeStateTimer.Stop();
        }
    }

    private void OnWantToAttackRangeBodyExited(Node2D body)
    {
        if (body is Player && State != EnemyState.Death)
        {
            _awaitingState = EnemyState.Shoot;
            _changeStateTimer.Start();
        }
    }

    private void OnAttackDecisionRangeBodyEntered(Node2D body)
    {
        if (body is Player && State != EnemyState.Kick)
        {
            if (GD.Randf() < 0.35f && IsOnFloor())
            {
                State = EnemyState.Dodge;
                SetDirectionTowardPlayer();

                Vector2 velocity = Velocity;
                velocity.X = (int)_moveDirection * Speed;
                velocity.Y = JumpVelocity;
                Velocity = velocity;

                PlayAnimation(MeowolasEnemyConstants.Animations.Dodge);

                CanBeHit = false;
            }
            else
            {
                Speed = 300.0f;
            }

            _shootCooldownTimer.Stop();
            _changeStateTimer.Stop();
        }
    }

    private void OnAttackDecisionRangeBodyExited(Node2D body)
    {
        if (body is Player && State != EnemyState.Kick && State != EnemyState.Dodge && State != EnemyState.TakeDamage && State != EnemyState.Death)
        {
            State = EnemyState.WantToAttack;
            Speed = 150.0f;
        }
    }

    //Start Kick:
    private void OnKickRangeBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            State = EnemyState.Kick;
            HandleKick();

            _shootCooldownTimer.Stop();
            _changeStateTimer.Stop();

            _kickCooldownTimer.Start();
        }
    }

    private void OnKickRangeBodyExited(Node2D body)
    {
        if (body is Player && State != EnemyState.Death)
        {
            _wasPlayerKickedOutOfKickRange = true;
            _kickCooldownTimer.Stop();
        }
    }

    private void OnKickCooldownTimerTimeout()
    {
        HandleKick();
    }


    //Set direction toward player:
    private void SetDirectionTowardPlayer()
    {
        if (Math.Abs(GlobalPosition.X - Player.CurrentGlobalPosition.X) > 10.0f && IsOnFloor())
        {
            if (GlobalPosition.X < Player.CurrentGlobalPosition.X)
            {
                _moveDirection = Direction.Right;
            }
            else
            {
                _moveDirection = Direction.Left;
            }
        }
    }

    //Handle when kick actually hits the Player:
    private void OnKickBodyEntered(Node2D body)
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

            EmitSignal(SignalName.EnemyHitPlayer, hitInfo);
        }
    }

    //For animation Player to safely enable and disable collisions:
    private void EnableKickCollision()
    {
        GetNode<CollisionShape2D>("Kick/KickHitbox").SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
    }

    private void DisableKickCollision()
    {
        GetNode<CollisionShape2D>("Kick/KickHitbox").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
    }


    //After TakeDamage / Kick animation finish continue:
    private void OnAnimatedSprite2DAnimationFinished()
    {
        if ((State == EnemyState.Kick && _wasPlayerKickedOutOfKickRange)
            || State == EnemyState.TakeDamage
            || State == EnemyState.Dodge)
        {
            State = EnemyState.WantToAttack;
            Speed = 150.0f;

            _wasPlayerKickedOutOfKickRange = false;
            CanBeHit = true;
        }
    }

    //To change state after a little delay to make it look more natural (in this case only for Shoot and only when entering ShootRange):
    private void OnChangeStateTimerTimeout()
    {
        if (_awaitingState == EnemyState.Shoot)
        {
            State = _awaitingState;
            _shootCooldownTimer.Start();

            PlayAnimation(MeowolasEnemyConstants.Animations.Attack2);
            _animatedSprite2D.Frame = 6;
        }
    }
}
