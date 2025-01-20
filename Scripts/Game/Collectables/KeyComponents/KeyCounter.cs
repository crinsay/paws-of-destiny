using Godot;
using System;
using System.ComponentModel;

namespace PawsOfDestiny.Scripts.Game.Collectables.KeyComponents;


public partial class KeyCounter : CanvasLayer
{
	private Label _keyCounterValue;
	private TextureRect _keyCounterTexture;

	public override void _Ready()
	{
		_keyCounterValue = GetNode<Label>(KeyCounterConstants.Nodes.KeyCounterValue);
        _keyCounterTexture = GetNode<TextureRect>(KeyCounterConstants.Nodes.KeyCounterTexture);

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
