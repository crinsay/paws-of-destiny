using Godot;
using PawsOfDestiny.Scripts.Common;
using PawsOfDestiny.Scripts.PlayerComponents;
using PawsOfDestiny.Singletons;
using System;

public partial class GroundSpikes : Area2D
{
    private int _damage = 1;
    private GameManager _gameManager;

    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager");
    }

    private void OnGroundSpikesBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            var hitInfo = new HitInformation
            {
                Body = body,
                Damage = _damage
            };

            _gameManager.OnSpikesHitPlayer(hitInfo);
        }
    }
}
