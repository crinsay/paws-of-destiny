using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawsOfDestiny.Scripts.Enemies.MeowolasEnemyComponents;

public class MeowolasEnemyConstants
{
    public class Nodes
    {
        public const string RightRayCast2D = "RightRayCast2D";
        public const string LeftRayCast2D = "LeftRayCast2D";
        public const string AnimatedSprite2D = "AnimatedSprite2D";
        public const string ShootCooldownTimer = "ShootCooldownTimer";
        public const string HealthBar = "HealthBar";
        public const string DeathTimer = "DeathTimer";
    }

    public class Animations
    {
        public const string Idle = "Idle";
        public const string Jump = "Jump";
        public const string Run = "Run";
        public const string Death = "Death";
        public const string TakeDamage = "TakeDamage";
        public const string Dodge = "Dodge";
        public const string Attack1 = "Attack1";
        public const string Attack2 = "Attack2";
        public const string Attack3 = "Attack3";
        public const string Attack4 = "Attack4";
    }
}

public enum EnemyState
{
    Idle,
    Running,
    Jumping,
    Attacking,
    TakingDamage,
    Dodging,
    Dead,
    JustHit
}
