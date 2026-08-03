using Godot;
using System;

public class DemonSpawner : SleepNode
{
    private PackedScene demonScene;
    private PackedScene demonCreeperScene;
    private int swaps;
    private Random random = new Random();
    public override void _Ready()
    {
        demonScene = GD.Load<PackedScene>("res://Demons/Demon.tscn");
        demonCreeperScene = GD.Load<PackedScene>("res://Demons/DemonCreeper.tscn");
        swaps = 0;
    }

    protected override void UpdateGameState(GameState newGameState)
    {
        swaps ++;
        // Spawn four demons
        if (newGameState == GameState.Dreaming)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 spawnPoint = GetEdgeSpawnpoint();

                Demon newDemon = demonScene.Instance<Demon>();
                newDemon.Position = spawnPoint;
                GetTree().Root.AddChild(newDemon);
            }
        }
        if (newGameState == GameState.Awake && swaps > 1)
        {
                Vector2 spawnPoint = GetEdgeSpawnpoint();

                Demon newDemon = demonCreeperScene.Instance<Demon>();
                newDemon.Position = spawnPoint;
                GetTree().Root.AddChild(newDemon);
        }
    }

    private Vector2 GetEdgeSpawnpoint()
    {
        int spawnSide = random.Next(4);
        if (spawnSide == 0)
            return new Vector2(-40, random.Next(0, GameSettings.ScreenHeight));
        else if (spawnSide == 2)
            return new Vector2(GameSettings.ScreenWidth + 40, random.Next(0, GameSettings.ScreenHeight));
        else if (spawnSide == 1)
            return new Vector2(random.Next(0, GameSettings.ScreenWidth), -40);
        else
            return new Vector2(random.Next(0, GameSettings.ScreenWidth), GameSettings.ScreenHeight + 40);
    
    }
}
