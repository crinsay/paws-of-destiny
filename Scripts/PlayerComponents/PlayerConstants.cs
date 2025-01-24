using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawsOfDestiny.Scripts.PlayerComponents;

public static class PlayerConstants
{
    public class Nodes
    {
        public const string AnimatedSprite2D = "AnimatedSprite2D";
        public const string Sword = "Sword";
        public const string AnimationPlayer = "AnimationPlayer";
    }

    public static class Animations
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

        //Animations for "Animation player" node:
        public const string BasicAttack = "BasicAttack";
    }
}


public enum PlayerState
{
    Idle,
    Running,
    Jumping,
    Attacking,
    TakingDamage,
    Dodging,
    Dead
}
