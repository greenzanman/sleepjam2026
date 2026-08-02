using Godot;
using System;

public class FishSpawner : Node
{
    private float greatestDepth;
    private float nextDepthSpawn;
    private Random rand = new Random();

    private PackedScene fishPrefab;
    public override void _Ready()
    {
        greatestDepth = GameManager.GetDepth();
        nextDepthSpawn = 20;
    
        fishPrefab = GD.Load<PackedScene>("res://Fish/BasicFish/BasicFish.tscn");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        float currentDepth = GameManager.GetDepth();
        greatestDepth = Mathf.Max(currentDepth, greatestDepth);

        if (greatestDepth >= nextDepthSpawn)
        {
            GD.Print("Spawning Fish");
            FishBase newFish = fishPrefab.Instance<FishBase>();
            newFish.Initialize(new Vector2( rand.Next(0, GameSettings.ScreenWidth), 
                nextDepthSpawn + GameSettings.ScreenHeight), rand.NextDouble());
            nextDepthSpawn += rand.Next(60, 125);
            GetTree().Root.AddChild(newFish);
        }
    }
}
