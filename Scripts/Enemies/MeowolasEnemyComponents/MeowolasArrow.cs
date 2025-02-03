using Godot;
using PawsOfDestiny.Scripts.Common;
using PawsOfDestiny.Scripts.Enemies.MeowtarTheBlueComponents;
using PawsOfDestiny.Scripts.PlayerComponents;
using System;

namespace PawsOfDestiny.Scripts.Enemies.MeowolasEnemyComponents;

public partial class MeowolasArrow : Node2D
{
	[Signal]
    public delegate void EnemyHitPlayerEventHandler(HitInformation hitInfo);

    [Export]
	public float Speed = 360.0f;
    [Export]
    public float KnockbackStrength = 60.0f;

    [Export]
    public int Damage = 1;

    public Vector2 Direction = Vector2.Zero;

	public override void _Ready()
	{
        Rotation = Direction.Angle();
    }

    public override void _Process(double delta)
	{
		Position += Direction * Speed * (float)delta;
    }

	private void OnArrowHitBoxBodyEntered(Node2D body)
	{
        if (body is not MeowolasEnemy && body is not MeowtarTheBlueEnemy)
		{
			if (body is Player)
			{
                var hitInfo = new HitInformation
                {
                    Body = body,
                    Damage = Damage,
                    KnockbackStrength = KnockbackStrength,
                    KnockbackDirection = Player.CurrentGlobalPosition.X < GlobalPosition.X ? Common.Direction.Left : Common.Direction.Right
                };
                EmitSignal(SignalName.EnemyHitPlayer, hitInfo);
            }

            QueueFree();
        }
    }
}
