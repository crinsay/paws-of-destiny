using Godot;
using PawsOfDestiny.Scripts.Game.Collectables.KeyComponents;
using System;

namespace PawsOfDestiny.Scripts.Game.GameManagerComponents;

public partial class GameManager : Node
{
    private KeyCounter _keyCounter;

    private int _collectedKeys;

    public override void _Ready()
    {
        _keyCounter = GetNode<KeyCounter>(GameManagerConstants.KeyCounter);
    }

    public void AddKey()
    {
        _collectedKeys += 1;
        _keyCounter.UpdateKeyCounter(_collectedKeys);
    }
}
