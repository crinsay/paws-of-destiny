using Godot;
using System;
using System.ComponentModel;

namespace PawsOfDestiny.Scripts.Game.Collectables.KeyComponents;


public partial class KeyCounter : CanvasLayer
{
	private Label _keyCounterValue;

	public override void _Ready()
	{
		_keyCounterValue = GetNode<Label>(KeyCounterConstants.Nodes.KeyCounterValue);

		_keyCounterValue.Text = "0";
    }

	public override void _Process(double delta)
	{
	}


	public void UpdateKeyCounter(int counter)
	{
		_keyCounterValue.Text = counter.ToString();
	}
}
