using Godot;
using System;
using System.Collections.Generic;

// TODO: This code is all very temp
public class DemonSpawner : SleepNode
{
    private PackedScene demonSneakerScene;
    private PackedScene demonBasicScene;
    private PackedScene demonCloseScene;
    private PackedScene demonSheepScene;
    private PackedScene sheepScene;
    private PackedScene demonCreeperScene;
    private PackedScene demonSleeperCircleScene;
    private PackedScene demonSleeperRectScene;

    private int swaps; // How many day/night swaps have happened
    private Random random = new Random();
    public static DemonSpawner Instance { get; private set; }

    public HashSet<int> sleepSpots = new HashSet<int>();

    // Tieing to sleep
    private int previousSleepiness; // For callbacks on sleepiness increases

    public override void _Ready()
    {
        Instance = this;

        demonBasicScene = GD.Load<PackedScene>("res://Demons/DemonBasic.tscn");
        demonCloseScene = GD.Load<PackedScene>("res://Demons/DemonClose.tscn");
        demonSheepScene = GD.Load<PackedScene>("res://Demons/DemonSheep.tscn");
        demonCreeperScene = GD.Load<PackedScene>("res://Demons/DemonCreeper.tscn");
        demonSneakerScene = GD.Load<PackedScene>("res://Demons/DemonSneaker.tscn");
        demonSleeperCircleScene = GD.Load<PackedScene>("res://Demons/DemonSleep.tscn");
        demonSleeperRectScene = GD.Load<PackedScene>("res://Demons/DemonSleepRect.tscn");
        sheepScene = GD.Load<PackedScene>("res://Sheep/Sheep.tscn");
        swaps = 0;
        previousSleepiness = 0;

        // Ready() calls UpdateGameState, so has to go second
        base._Ready();
    
    
    }

    protected override void ProcessAwake(float delta)
    {
        float currentSleepiness = GameManager.GetSleepCount();
        if (currentSleepiness >= previousSleepiness + 1)
        {
            for (int i = previousSleepiness + 1; i < currentSleepiness; i++)
            {
                previousSleepiness+= 1;
                OnSleepiness(previousSleepiness);
            }
        }
    }

    public void SpawnBasicDemon()
    {
        (Vector2 spawnPoint, int side) = GetEdgeSpawnpoint();

            Demon newDemon;
            
            newDemon = random.Next(3) == 0 ? demonCloseScene.Instance<Demon>() : 
                demonBasicScene.Instance<Demon>();
            newDemon.Initialize(side);
            newDemon.Position = spawnPoint;
            GetTree().Root.AddChild(newDemon);
    }

    private void SpawnRebelliousSheep()
    {
        (Vector2 spawnPoint, _) = GetEdgeSpawnpoint();

        // TODO: Don't spawn near any existing creepers
        foreach (Demon demon in GameManager.GetDemons())
        {
            if (demon is DemonCreeper)
            {
                // Flip spawn side from creeper
                if (Mathf.Sign(demon.Position.x - GameSettings.ScreenWidth / 2)
                    == Mathf.Sign(spawnPoint.x - GameSettings.ScreenWidth / 2))
                    spawnPoint.x = GameSettings.ScreenWidth - spawnPoint.x;

                if (Mathf.Sign(demon.Position.y - GameSettings.ScreenHeight / 2)
                    == Mathf.Sign(spawnPoint.y - GameSettings.ScreenHeight / 2))
                    spawnPoint.y = GameSettings.ScreenHeight - spawnPoint.y;

                break; // Once there's more than one creeper, sucks to suck
            }
        }

        Sheep newSheep = sheepScene.Instance<Sheep>();
        newSheep.Position = spawnPoint;
        newSheep.rebellious = true;
        GetTree().Root.AddChild(newSheep);
    }

    // Callbacks when hitting specific sleepiness
    private void OnSleepiness(int value)
    {
        // On intervals TODO: flesh this out
        if (value < GameManager.MAX_SLEEPCOUNT && value % (GameManager.MAX_SLEEPCOUNT / 4) == 0)
        {
            SpawnBasicDemon();
        }

        if ( value == GameManager.MAX_SLEEPCOUNT / 3)
        {
            SpawnRebelliousSheep();
        }

        if ( value == GameManager.MAX_SLEEPCOUNT / 2)
        {
            SpawnSleeper();
        }
    }

    protected override void UpdateGameState(GameState newGameState)
    {
        swaps ++;

        if (newGameState == GameState.Awake)
        {
            previousSleepiness = 0;
        }

        // // Spawns that happen during swaps
        // if (newGameState == GameState.Awake && swaps > 1)
        // {
        //     SpawnCreeper();
        // }

        // if (newGameState == GameState.Awake && swaps > 1)
        // {

        // }
    }

    private void SpawnCreeper()
    {
        (Vector2 spawnPoint, int side) = GetEdgeSpawnpoint();

        Demon newDemon = demonSheepScene.Instance<Demon>();
        newDemon.Initialize(side);
        newDemon.Position = spawnPoint;
        GetTree().Root.AddChild(newDemon);
    }

    private void SpawnSneaker()
    {
        (Vector2 spawnPoint, _) = GetEdgeSpawnpoint();

        DemonSneaker newDemon = demonSneakerScene.Instance<DemonSneaker>();
        newDemon.Position = spawnPoint;
        GetTree().Root.AddChild(newDemon);
    }

    public void SpawnSleeper()
    {
        // Spots all filled already, somehow
        if (sleepSpots.Count == 8)
            return;
    
        // Find a position that's not taken
        int[] openSpots = new int[8 - sleepSpots.Count];
        int index = 0;
        for (int i = 0; i < 8; i++)
        {
            if (!sleepSpots.Contains(i))
            {
                openSpots[index] = i;
                index++;   
            }
        }

        int chosenSpot = openSpots[random.Next(openSpots.Length)];
        sleepSpots.Add(chosenSpot);

        const int sideOffset = 150;

        if (chosenSpot % 2 == 0)
        {
            DemonSleep demonSleep = demonSleeperCircleScene.Instance<DemonSleep>();
            bool left = chosenSpot % 4 == 0;
            bool top = chosenSpot < 4;

            Vector2 position = new Vector2(
                left ? sideOffset : GameSettings.ScreenWidth - sideOffset,
                top ? sideOffset : GameSettings.ScreenHeight - sideOffset
            );

            position += new Vector2(random.Next(-50, 50), random.Next(-50, 50));

            demonSleep.Circular = true;
            demonSleep.Position = position;
            demonSleep.radius = 50;
            demonSleep.positionIndex = chosenSpot;
            GetTree().Root.AddChild(demonSleep);
        }
        else
        {
            DemonSleep demonSleep = demonSleeperRectScene.Instance<DemonSleep>();
        
            // Top or bottom
            Vector2 position;

            if (chosenSpot % 4 == 1)
            {
                bool top = chosenSpot == 1;

                position = new Vector2(
                    GameSettings.ScreenWidth / 2,
                    top ? sideOffset : GameSettings.ScreenHeight - sideOffset
                );

                position += new Vector2(random.Next(-100, 100), random.Next(-30, 30));

                demonSleep.halfLength = 200;
                demonSleep.halfHeight = 40;
            }
            else
            {
                bool left = chosenSpot == 7;

                position = new Vector2(
                    left ? sideOffset : GameSettings.ScreenWidth - sideOffset,
                    GameSettings.ScreenHeight / 2
                );
                
                position += new Vector2(random.Next(-30, 30), random.Next(-100, 100));

                demonSleep.halfLength = 40;
                demonSleep.halfHeight = 160;
            }

            demonSleep.Circular = false;
            demonSleep.Position = position;
            demonSleep.positionIndex = chosenSpot;
            GetTree().Root.AddChild(demonSleep);
        }
    }

    const int outsidePadding = 40; // How far outside edges
    const int edgePadding = 100; // How far from corners
    private (Vector2, int) GetEdgeSpawnpoint()
    {
        int spawnSide = random.Next(4);
        if (spawnSide == 0)
            return (new Vector2(-outsidePadding, random.Next(edgePadding, GameSettings.ScreenHeight - edgePadding)), spawnSide);
        else if (spawnSide == 2)
            return (new Vector2(GameSettings.ScreenWidth + outsidePadding, random.Next(edgePadding, GameSettings.ScreenHeight - edgePadding)), spawnSide);
        else if (spawnSide == 1)
            return (new Vector2(random.Next(edgePadding, GameSettings.ScreenWidth - edgePadding), -outsidePadding), spawnSide);
        else
            return (new Vector2(random.Next(edgePadding, GameSettings.ScreenWidth - edgePadding), GameSettings.ScreenHeight + outsidePadding), spawnSide);
    
    }
}
