using Godot;
using PawsOfDestiny.Scripts.Game.GameManagerComponents;
using PawsOfDestiny.Scripts.PlayerComponents;
using System;

namespace PawsOfDestiny.Scripts.Enemies.MeowolasEnemyComponents;

public partial class MeowolasArrow : Node2D
{
	[Signal]
    public delegate void MeowolasArrowHitPlayerEventHandler(int damage);

    [Export]
	public float Speed = 360.0f;

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
        if (body is not MeowolasEnemy)
		{
			if (body is Player)
			{
				EmitSignal(SignalName.MeowolasArrowHitPlayer, Damage);
            }

            QueueFree();
        }
    }
}
