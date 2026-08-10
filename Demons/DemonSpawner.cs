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
    
    [Export] NodePath audioWolfPath;
    private AudioStreamPlayer audioWolf;

    private Random random = new Random();
    public static DemonSpawner Instance { get; private set; }

    public HashSet<int> sleepSpots = new HashSet<int>();

    // Tieing to sleep
    private int previousSleepiness; // For callbacks on sleepiness increases

    private enum sleepEvent
    {
        empty = 0,
        rebellious = 1,
        cursed = 2,
        demonBasic = 3,
        demonClose = 4,
        demonCreeper = 5,
        sleeper = 6,
        normalSheep = 7,
    }

    // This is prefilled every day with spawn events. Only one 'event' per breakpoint
    private int[] sleepBreakpoints = new int[GameManager.MAX_SLEEPCOUNT];

    public override void _Ready()
    {
        Instance = this;

        GameManager.Instance.ResetWorldRoot(); // Hacky fix to get restarts to work
        demonBasicScene = GD.Load<PackedScene>("res://Demons/DemonBasic.tscn");
        demonCloseScene = GD.Load<PackedScene>("res://Demons/DemonClose.tscn");
        demonSheepScene = GD.Load<PackedScene>("res://Demons/DemonSheep.tscn");
        demonCreeperScene = GD.Load<PackedScene>("res://Demons/DemonCreeper.tscn");
        demonSneakerScene = GD.Load<PackedScene>("res://Demons/DemonSneaker.tscn");
        demonSleeperCircleScene = GD.Load<PackedScene>("res://Demons/DemonSleep.tscn");
        demonSleeperRectScene = GD.Load<PackedScene>("res://Demons/DemonSleepRect.tscn");
        sheepScene = GD.Load<PackedScene>("res://Sheep/Sheep.tscn");
            
        //audioWolf = GetNode<AudioStreamPlayer>(audioWolfPath);

        previousSleepiness = 0;

        // Ready() calls UpdateGameState, so has to go second
        base._Ready();
    
    }

    protected override void ProcessAwake(float delta)
    {
        // Tracking sleep breakpoints
        float currentSleepiness = GameManager.GetSleepCount();
        if (currentSleepiness >= previousSleepiness + 1)
        {
            for (int i = previousSleepiness + 1; i < currentSleepiness; i++)
            {
                previousSleepiness += 1;
                OnSleepiness(previousSleepiness);
            }
        }
    }

    // Predetermine all sleep events

    const int rebelliousNight = 4;
    const int sleeperNight = 6;
    const int sleeperCost = 1;
    const int closeNight = 5;
    const int closeCost = 3;
    const int creeperNight = 3;
    const int creeperCost = 5;
    const int curseNight = 7;
    const int curseCost = 8;

    private void FillSleepEvents()
    {
        for (int i = 0; i < GameManager.MAX_SLEEPCOUNT; i++)
        {
            sleepBreakpoints[i] = 0;
        }

        int nightCount = GameManager.GetNightCount();

        // Nothing happens night 1, and this also interferes orderingwise
        if (nightCount == 0)
        {
            return;
        }

        int sheepCount = GameManager.GetSheep().Count;
        if (sheepCount == 0) // Temp fix for sheep not being initialized yet
            sheepCount = 100;

        // After night 4, rebellious sheep can bump up numbers
        bool shouldSpawnRebelliousSheep = nightCount >= rebelliousNight && sheepCount >= GameSettings.defaultSheep && sheepCount < GameSettings.maxSheep;
        bool shouldSpawnNormalSheep = sheepCount < GameSettings.defaultSheep;

        // After rebellious night * 2, only rebellious sheep
        if (nightCount >= rebelliousNight * 2 && shouldSpawnNormalSheep)
        {
            shouldSpawnRebelliousSheep = true;
            shouldSpawnNormalSheep = false;
        }

        // Disable sleepers, no existing sprites
        bool shouldSpawnSleepers = false; //nightCount > sleeperNight;
        bool shouldSpawnCreepers = nightCount >= creeperNight;
        bool shouldSpawnClose = nightCount >= closeNight;
        bool shouldCurseSheep = nightCount >= curseNight;

        // Limit number of creepers that can spawn
        int spawnedCreepers = 0;
        int spawnedBasic = 0;

        int demonBudget = Mathf.Max(0, nightCount * 2 - 1); // 0, 1, 3, 5, 7, etc.

        // Sheep spawns are highest priority
        if (shouldSpawnNormalSheep)
        {
            SetNthBreakpoint(random.Next(4), sleepEvent.normalSheep);
            if (sheepCount == 1) // Don't drop below 3 at a time, maybe?
                SetNthBreakpoint(random.Next(4), sleepEvent.normalSheep);
        }
        else if (shouldSpawnRebelliousSheep)
        {
            SetNthBreakpoint(random.Next(4), sleepEvent.rebellious);
            if (sheepCount == 1) // Don't drop below 3 at a time, maybe?
                SetNthBreakpoint(random.Next(4), sleepEvent.rebellious);
        }

        // Total number of demon spawns
        if (shouldSpawnCreepers)
        {
            // Always spawn creepers, except for first curse night
            if (nightCount == curseNight)
            {
                shouldSpawnCreepers = false;
            }
            else
            {
                SetNthBreakpoint(random.Next(4, 8), sleepEvent.demonCreeper);
                demonBudget -= creeperCost;
            }
        }

        // Maybe spawn more if there are fewer sheep?
        if (shouldSpawnSleepers)
        {
            // Always spawn on first night they're available
            if (nightCount == sleeperNight || random.Next(2) == 0
             || nightCount > sleeperNight * 2)
            {
                SetNthBreakpoint(random.Next(4, 8), sleepEvent.sleeper);
            }
            //shouldSpawnSleepers = false;
            
            // sleepers don't incur a cost
            // demonBudget -= sleeperCost;
            //}
        }

        if (shouldCurseSheep)
        {
            // First night, or 33% change
            if (nightCount == curseNight || random.Next(3) == 0
                || nightCount >= curseNight * 2) // Force spawn after night 14
            {
                SetNthBreakpoint(random.Next(2, 6), sleepEvent.cursed);
                demonBudget -= curseCost;
            }
        }

        // Spawning remaining demons
        while (demonBudget > 0)
        {
            // Close, creepers, and normal demons
            int availableSpots = 0;
            foreach (int breakpoint in sleepBreakpoints)
            {
                if (breakpoint == 0)
                    availableSpots++;
            }

            int index = random.Next(Mathf.Max(0, availableSpots - 5));
            sleepEvent sleepEvent = sleepEvent.demonBasic;
            int cost = 1;

            if (shouldSpawnCreepers && spawnedCreepers < nightCount / 3
                 && demonBudget >= creeperCost && random.Next(4) == 0)
            {
                cost = creeperCost;
                sleepEvent = sleepEvent.demonCreeper;   
            }
            else if (shouldSpawnClose && demonBudget >= closeCost && random.Next(3) == 0)
            {
                cost = closeCost;
                sleepEvent = sleepEvent.demonClose;
            }

            if (!SetNthBreakpoint(index, sleepEvent))
                break;

            // Don't spawn too many basic demons
            if (sleepEvent == sleepEvent.demonBasic || sleepEvent == sleepEvent.demonClose)
            {
                spawnedBasic++;
                if (spawnedBasic >= 6)
                    break;
            }

            demonBudget -= cost;
        }
#if DEBUG
        string breakpoints = "";
        foreach (int breakpoint in sleepBreakpoints)
        {
            breakpoints += breakpoint.ToString();
        }
        GD.Print($"Breakpoints: {breakpoints}");
#endif
    }

    private bool SetNthBreakpoint(int index, sleepEvent sleepEvent)
    {
        int count = 0;
        for (int i = 0; i < GameManager.MAX_SLEEPCOUNT; i++)
        {
            if (sleepBreakpoints[i] == 0)
            {
                count++;
                if (count > index)
                {
                    sleepBreakpoints[i] = (int) sleepEvent;
                    return true;
                }
            }
        }
        return false;
    }

    // Callbacks when hitting specific sleepiness
    private void OnSleepiness(int value)
    {
        if (value < 0 || value >= GameManager.MAX_SLEEPCOUNT)
            return;

        switch (sleepBreakpoints[value])
        {
            case (int) sleepEvent.rebellious:
            SpawnSheep(true);
            break;
            case (int) sleepEvent.normalSheep:
            SpawnSheep(false);
            break;
            case (int) sleepEvent.demonBasic:
            SpawnBasicDemon();
            break;
            case (int) sleepEvent.demonClose:
            SpawnCloseDemon();
            break;
            case (int) sleepEvent.demonCreeper:
            SpawnCreeper();
            break;
            case (int) sleepEvent.sleeper:
            SpawnSleeper();
            break;
            case (int) sleepEvent.cursed:
            SpawnCursedSheep();
            break;
            default:
            break;
        }
    }

    private void NightStartSpawns()
    {
        
    }

    public void SpawnBasicDemon()
    {
        (Vector2 spawnPoint, int side) = GetEdgeSpawnpoint();
        
        Demon newDemon = demonBasicScene.Instance<Demon>();
        newDemon.Initialize(side);
        newDemon.Position = spawnPoint;
        GameManager.WorldRoot.AddChild(newDemon);
    }

    private void SpawnCloseDemon()
    {
        
        (Vector2 spawnPoint, int side) = GetEdgeSpawnpoint();
        
        Demon newDemon = demonCloseScene.Instance<Demon>();
        newDemon.Initialize(side);
        newDemon.Position = spawnPoint;
        GameManager.WorldRoot.AddChild(newDemon);
    }

    private void SpawnSheep( bool rebellious )
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
        newSheep.rebellious = rebellious;
        GameManager.WorldRoot.AddChild(newSheep);
    }

    protected override void UpdateGameState(GameState newGameState)
    {

        if (newGameState == GameState.Awake)
        {
            FillSleepEvents();
            previousSleepiness = -1;
        }

        NightStartSpawns();
    }

    private void SpawnCreeper()
    {
        (Vector2 spawnPoint, int side) = GetEdgeSpawnpoint();

        Demon newDemon = demonCreeperScene.Instance<Demon>();
        newDemon.Initialize(side);
        newDemon.Position = spawnPoint;
        GameManager.WorldRoot.AddChild(newDemon);
    }

    private void SpawnCursedSheep()
    {
        (Vector2 spawnPoint, int side) = GetEdgeSpawnpoint();

        Demon newDemon = demonSheepScene.Instance<Demon>();
        newDemon.Initialize(side);
        newDemon.Position = spawnPoint;
        GameManager.WorldRoot.AddChild(newDemon);
    }

    private void SpawnSneaker()
    {
        (Vector2 spawnPoint, _) = GetEdgeSpawnpoint();

        DemonSneaker newDemon = demonSneakerScene.Instance<DemonSneaker>();
        newDemon.Position = spawnPoint;
        GameManager.WorldRoot.AddChild(newDemon);
    }

    public void SpawnSleeper()
    {
        // Spots all filled already, somehow
        if (sleepSpots.Count == 7)
            return;
    
        // Find a position that's not taken
        int[] openSpots = new int[7 - sleepSpots.Count];
        int index = 0;
        for (int i = 0; i < 8; i++)
        {
            // No longer use top sleeper
            if (i == 1)
                continue;
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
            GameManager.WorldRoot.AddChild(demonSleep);
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
            GameManager.WorldRoot.AddChild(demonSleep);
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
