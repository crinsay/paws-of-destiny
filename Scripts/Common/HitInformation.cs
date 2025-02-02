using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawsOfDestiny.Scripts.Common;

public partial class HitInformation : GodotObject
{
    public int Damage { get; init; }
    public float KnockbackStrength { get; init; }
    public Direction KnockbackDirection { get; init; }
}
