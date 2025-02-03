using Godot;
using System;

namespace PawsOfDestiny.Scripts.Common.Components;

public partial class HealthBar : ProgressBar
{
	private double _health;
	public double Health
	{
		get => _health;
		set
		{
            var previousHealth = _health;

            _health = Math.Min(MaxValue, value);
            Value = _health;

            if (_health < previousHealth)
            {
                _damageTimer.Start();
            }
            else
            {
                _damageBar.Value = _health;
            }
        }
	}

    private ProgressBar _damageBar;
    private Timer _damageTimer;

	public override void _Ready()
	{
		_damageBar = GetNode<ProgressBar>("DamageBar");
        _damageTimer = GetNode<Timer>("DamageTimer");
    }

    public void InitializeHealthBarComponent(double maxHealth, double currentHealth)
    {
        Health = currentHealth;

        MaxValue = maxHealth;
        Value = Health;

        _damageBar.MaxValue = maxHealth;
        _damageBar.Value = Health;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnDamageTimerTimeout()
	{
        _damageBar.Value = Health;
    }
}
