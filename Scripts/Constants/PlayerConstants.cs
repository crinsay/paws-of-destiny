using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawsOfDestiny.Scripts.Constants;

internal static class PlayerConstants
{
    internal class Nodes
    {
        internal const string AnimatedSprite2D = "AnimatedSprite2D";
    }

    internal static class InputActions
    {
        internal static string MoveLeft = "MoveLeft";
        internal static string MoveRight = "MoveRight";
        internal static string Jump = "Jump";
        internal static string MoveDown = "MoveDown";
    }

    internal static class Animations
    {
        internal const string Idle = "Idle";
        internal const string Jump = "Jump";
        internal const string Run = "Run";
        internal const string Death = "Death";
        internal const string TakeDamage = "TakeDamage";
        internal const string Dodge = "Dodge";
        internal const string Attack1 = "Attack1";
        internal const string Attack2 = "Attack2";
        internal const string Attack3 = "Attack3";
        internal const string Attack4 = "Attack4";
    }
}
