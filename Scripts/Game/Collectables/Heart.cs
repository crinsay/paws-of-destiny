using Godot;
using PawsOfDestiny.Scripts.PlayerComponents;
using PawsOfDestiny.Singletons;

namespace PawsOfDestiny.Scripts.Game.Collectables;

public partial class Heart : Area2D
{
    private GameManager _gameManager;
    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager");
        GetNode<AnimatedSprite2D>("AnimatedSprite2D").Modulate = new Color(1.5f, 1.5f, 1.5f);
    }

    private void OnHeartBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            var player = body as Player;
            if (player.Health < 9)
            {
                _gameManager.OnHeartCollected(body);
                QueueFree();
            }
        }
    }
}
