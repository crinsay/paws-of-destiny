using Godot;
using System;

public partial class Doors : Area2D
{
    private GameManagerSingleton _gameManagerSingleton;

    public override void _Ready()
    {
        _gameManagerSingleton = GetNode<GameManagerSingleton>("/root/GameManagerSingleton");
        //gameManagerSingleton.DoorsCreated(this);
    }

    private void OnDoorsBodyEntered(Node2D node)
    {
        GD.Print("on area entered");
        _gameManagerSingleton.LoadNextLevel();
    }
}
