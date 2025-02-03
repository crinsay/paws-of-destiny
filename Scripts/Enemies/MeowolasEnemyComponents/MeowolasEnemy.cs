using Godot;
using PawsOfDestiny.Scripts.Common;
using PawsOfDestiny.Scripts.Common.Components;
using PawsOfDestiny.Scripts.Enemies.MeowtarTheBlueEnemyComponents;
using PawsOfDestiny.Scripts.PlayerComponents;
using PawsOfDestiny.Singletons;
using System;

namespace PawsOfDestiny.Scripts.Enemies.MeowolasEnemyComponents;

public partial class MeowolasEnemy : CharacterBody2D
{
    public MeowolasState State { get; private set; } = MeowolasState.Patrol;
    public bool CanBeHit { get; private set; } = true;
    public int Health { get; private set; }

    [Export]
    public PackedScene ArrowsScene { get; set; }

    [Export]
    public float Speed = 50.0f;

    [Export]
    public float JumpVelocity = -275.0f;


    private GameManager _gameManager;

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
    private MeowolasState _awaitingState;
    private HitInformation _hitInfo;

    private bool _isEnemyJustHit = false;
    private bool _wasPlayerKickedOutOfKickRange = false;
    private Label _stateForDebug;

    private MeowolasEnemyStats _meowolasStats;

    public override void _Ready()
	{
        _gameManager = GetNode<GameManager>("/root/GameManager");
        _gameManager.Connect(GameManager.SignalName.PlayerHitMeowolasEnemy,
            new Callable(this, nameof(OnGameManagerPlayerHitMeowolasEnemy)));
        _gameManager.Connect(GameManager.SignalName.MeowolasEnemyRunAway,
            new Callable(this, nameof(OnGameManagerMeowolasEnemyRunAway)));

        _meowolasStats = GetNode<MeowolasEnemyStats>("/root/MeowolasEnemyStats");
        Health = _meowolasStats.Health;

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

        _healthBar.InitializeHealthBarComponent(9.0d, Health);
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

        switch(State)
        {
            case MeowolasState.Patrol:
                HandlePatrol(ref velocity);
                break;
            case MeowolasState.Chase:
                HandleChase(ref velocity);
                break;
            case MeowolasState.Shoot:
                HandleShoot(ref velocity);
                break;
            case MeowolasState.WantToAttack:
                HandleWantToAttack(ref velocity);
                break;
            case MeowolasState.RunAway:
                HandleRunAway(ref velocity);
                break;
            case MeowolasState.Death:
                HandleDeath(ref velocity);
                break;
            default:
                break;
        }
        
        Velocity = velocity;
        MoveAndSlide();
    }

    //Handle every behavior:
    private void HandlePatrol(ref Vector2 velocity)
    {
        _stateForDebug.Text = nameof(MeowolasState.Patrol);
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
        _stateForDebug.Text = nameof(MeowolasState.Chase);
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
        _stateForDebug.Text = nameof(MeowolasState.Shoot);
        velocity.X = 0.0f;

        //Animation and direction:
        SetDirectionTowardPlayer();
        PlayAnimation(MeowolasEnemyConstants.Animations.Attack2);
    }

    private void HandleWantToAttack(ref Vector2 velocity)
    {
        _stateForDebug.Text = nameof(MeowolasState.WantToAttack);
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
        _stateForDebug.Text = nameof(MeowolasState.Kick);

        Vector2 velocity = Velocity;
        velocity.X = 0.0f;
        Velocity = velocity;

        //Animation and direction:
        SetDirectionTowardPlayer();
        _animationPlayer.Stop();
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

    private void HandleRunAway(ref Vector2 velocity)
    {
        _stateForDebug.Text = nameof(MeowolasState.RunAway);
        velocity.X = (int)_moveDirection * Speed;
        if (_rightRayCast2D.IsColliding() && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }
        else if (_leftRayCast2D.IsColliding() && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        PlayAnimation(MeowolasEnemyConstants.Animations.Run);
    }

    private void HandleDeath(ref Vector2 velocity)
    {
        _stateForDebug.Text = nameof(MeowolasState.Death);
        velocity.X = Mathf.MoveToward(Velocity.X, 0f, Speed);
    }


    //Enemy got hit by the Player (because basically only Player can hit him):
    private void OnGameManagerPlayerHitMeowolasEnemy(HitInformation hitInfo)
    {
        _hitInfo = hitInfo;

        _meowolasStats.Health -= _hitInfo.Damage;
        Health = _meowolasStats.Health;
        _healthBar.Health = Health;
        if (Health > 0)
        {
            _stateForDebug.Text = nameof(MeowolasState.TakeDamage);
            State = MeowolasState.TakeDamage;
            PlayAnimation(MeowolasEnemyConstants.Animations.TakeDamage);
            _isEnemyJustHit = true;
        }
        else
        {
            _stateForDebug.Text = nameof(MeowolasState.Death);         
            State = MeowolasState.Death;
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
        _gameManager.OnMeowolasEnemyNewArrowInstantiated(arrow);
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
            State = MeowolasState.Chase;
            Speed = 100.0f;
        }
    }

    //Player lost from sight, so start patroling again:
    private void OnSightRangeBodyExited(Node2D body)
    {
        if (body is Player && State != MeowolasState.Death && State != MeowolasState.RunAway)
        {
            State = MeowolasState.Patrol;
            Speed = 50.0f;
        }
    }

    //Start Shoot (after little delay):
    private void OnShootRangeBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            _awaitingState = MeowolasState.Shoot;
            _changeStateTimer.Start();
        }
    }

    //Stop shooting and start chasing again:
    private void OnShootRangeBodyExited(Node2D body)
    {
        if (body is Player && State != MeowolasState.Death && State != MeowolasState.RunAway)
        {
            Speed = 100.0f;
            State = MeowolasState.Chase;

            //Stop shooting:
            _shootCooldownTimer.Stop();
            _changeStateTimer.Stop();
        }
    }

    private void OnWantToAttackRangeBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            State = MeowolasState.WantToAttack;
            Speed = 150.0f;

            //Stop shooting:
            _shootCooldownTimer.Stop();
            _changeStateTimer.Stop();
        }
    }

    private void OnWantToAttackRangeBodyExited(Node2D body)
    {
        if (body is Player && State != MeowolasState.Death && State != MeowolasState.RunAway)
        {
            _awaitingState = MeowolasState.Shoot;
            _changeStateTimer.Start();
        }
    }

    private void OnAttackDecisionRangeBodyEntered(Node2D body)
    {
        if (body is Player && State != MeowolasState.Kick)
        {
            if (GD.Randf() < 0.35f && IsOnFloor())
            {
                State = MeowolasState.Dodge;
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
        if (body is Player && State != MeowolasState.Kick && State != MeowolasState.Dodge && State != MeowolasState.TakeDamage && State != MeowolasState.Death && State != MeowolasState.RunAway)
        {
            State = MeowolasState.WantToAttack;
            Speed = 150.0f;
        }
    }

    //Start Kick:
    private void OnKickRangeBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            State = MeowolasState.Kick;
            HandleKick();

            _shootCooldownTimer.Stop();
            _changeStateTimer.Stop();

            _kickCooldownTimer.Start();
        }
    }

    private void OnKickRangeBodyExited(Node2D body)
    {
        if (body is Player && State != MeowolasState.Death && State != MeowolasState.RunAway)
        {
            _wasPlayerKickedOutOfKickRange = true;
            _kickCooldownTimer.Stop();
        }
    }

    private void OnKickCooldownTimerTimeout()
    {
        HandleKick();
    }

    private void OnGameManagerMeowolasEnemyRunAway()
    {
        State = MeowolasState.RunAway;
        Speed = 150.0f;
        SetDirectionTowardDoors();
        _animatedSprite2D.Modulate = new Color(1.2f, 1.2f, 1.2f);
        
        GetNode<CollisionShape2D>("SightRange/CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        GetNode<CollisionShape2D>("ShootRange/CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        GetNode<CollisionShape2D>("WantToAttackRange/CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        GetNode<CollisionShape2D>("AttackDecisionRange/CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        GetNode<CollisionShape2D>("KickRange/CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        GetNode<CollisionShape2D>("Kick/KickHitbox").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);

        _animationPlayer.Stop();
        _shootCooldownTimer.Stop();
        _kickCooldownTimer.Stop();
        _changeStateTimer.Stop();

        CanBeHit = false;
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

    private void SetDirectionTowardDoors()
    {
        var doorsPosition = GetNode<Doors>("%Doors").GlobalPosition;
        GlobalPosition.DirectionTo(doorsPosition);
        if (GlobalPosition.X < doorsPosition.X)
        {
            _moveDirection = Direction.Right;
        }
        else
        {
            _moveDirection = Direction.Left;
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

            _gameManager.OnEnemyHitPlayer(hitInfo);
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
        if ((State == MeowolasState.Kick && _wasPlayerKickedOutOfKickRange)
            || State == MeowolasState.TakeDamage
            || State == MeowolasState.Dodge)
        {
            State = MeowolasState.WantToAttack;
            Speed = 150.0f;

            _wasPlayerKickedOutOfKickRange = false;
            CanBeHit = true;
        }
    }

    //To change state after a little delay to make it look more natural (in this case only for Shoot and only when entering ShootRange):
    private void OnChangeStateTimerTimeout()
    {
        if (_awaitingState == MeowolasState.Shoot)
        {
            State = _awaitingState;
            _shootCooldownTimer.Start();

            PlayAnimation(MeowolasEnemyConstants.Animations.Attack2);
            _animatedSprite2D.Frame = 6;
        }
    }
}
