using Godot;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

public enum GameState
{
    Awake = 0,
    Dreaming = 1
}

public class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    public static bool DebugDraw = false;

    private Player player;
    private float gameTime;

    // Generic Game Settings
    private float timeDilation = 1;
    private bool isPaused = false; 

    // Sleep settings
    private GameState gameState = GameState.Awake;
    // Increments whenever a sheep jumps
    private float sleepCount = 0;
    public const int MAX_SLEEPCOUNT = 24;
    private float wakeRate = 1; // Increases once there are no demons
    private bool cyclePaused = false;

    // Sheep
    private HashSet<Sheep> sheep = new HashSet<Sheep>();

    // Demons
    private HashSet<Demon> demons = new HashSet<Demon>();

    public override void _Ready()
    {
        Instance = this;
        timeDilation = 1;
        isPaused = false;
        gameTime = 0;
    }

    public override void _Process(float delta)
    {
        float trueDelta = delta * GetTimeDilation();
        gameTime += trueDelta;
        ProcessSleep(trueDelta);
#if DEBUG
        ProcessDebug(trueDelta);
#endif
        ProcessCleanup(trueDelta);
    }

// MARK: Getters and Setters
    public static void SetPlayer( Player inPlayer ) { Instance.player = inPlayer; }
    public static Player GetPlayerWorldPosition() { return Instance.GetPlayerInternal(); }
    public Player GetPlayerInternal() { 
        if (player == null)
        {
            GD.PrintErr("Attempted to get player position before it's set");
            return null;
        }
        return player;
    }

    public static bool GetPaused() { return Instance.isPaused; }

    public static float GetTimeDilation()
    {
        if (Instance.isPaused)
            return 0;

        return Instance.timeDilation;
    }

    public static float GetGameTime()
    {
        return Instance.gameTime;
    }
    
    public static GameState GetGameState() {return Instance.gameState;}
    public static void SetGameState( GameState newGameState ) { Instance.SetGameStateInternal( newGameState ); }
    private void SetGameStateInternal( GameState newGameState )
    {
        gameState = newGameState;
        // Do other stuff here. Send delegate alert to all nodes?
    }

    public static void AddSheep( Sheep newSheep ) { Instance.sheep.Add(newSheep); }
    public static void RemoveSheep( Sheep oldSheep ) { Instance.sheep.Remove(oldSheep); }
    public static ref readonly HashSet<Sheep> GetSheep() { return ref Instance.sheep;} 
    
    public static void AddDemon( Demon newDemon ) { Instance.demons.Add(newDemon); }
    public static void RemoveDemon( Demon oldDemon ) { Instance.demons.Remove(oldDemon); }
    public static ref readonly HashSet<Demon> GetDemons() { return ref Instance.demons;} 

// MARK: Sleep stuff
    public static void IncrementSleepCount( int amount = 1) { 
#if DEBUG
    if (!Instance.cyclePaused)
#endif
        Instance.sleepCount = Math.Min(Instance.sleepCount + amount, MAX_SLEEPCOUNT); 
    }
    public static float GetSleepCount() { return Instance.sleepCount; }

// MARK: Process functions
    private void ProcessSleep(float delta)
    {
        if (gameState == GameState.Awake)
        {
            if (sleepCount >= MAX_SLEEPCOUNT)
            {
                gameState = GameState.Dreaming;
                wakeRate = 1;
            }
        }
        else
        {
#if DEBUG
            if (cyclePaused) sleepCount += delta;
#endif
            sleepCount -= delta * wakeRate;

            // Sleep decreases faster if nothing to do
            if (demons.Count == 0)
                wakeRate += delta;

            if (sleepCount <= 0)
            {
                sleepCount = 0;
                gameState = GameState.Awake;
            }
        }
    }
    private void ProcessDebug(float delta)
    {
        string pauseString = isPaused ? "\nPAUSED" : "";
        DebugManager.SetDebugText(pauseString);

        if (Input.IsActionJustPressed("key_debugPauseCycle"))
        {
            GD.Print("Toggling Cycle Pause");
            cyclePaused = !cyclePaused;
        }
        if (Input.IsActionJustPressed("key_debugIncrementSleep"))
        {
            //GD.Print("Incrementing sleep count");
            sleepCount += 2;
        }

        if (Input.IsActionJustPressed("key_debugDecrementSleep"))
        {
            //GD.Print("Decrementing sleep count");
            sleepCount = Mathf.Max(0, sleepCount - 2);
        }

        if (Input.IsActionJustPressed("key_pause"))
        {
            GD.Print("Toggling pause");
            isPaused = !isPaused;
        }

        if (Input.IsActionJustPressed("key_debugDraw"))
        {
            DebugDraw = !DebugDraw;
        }
    }

    private void ProcessCleanup(float delta)
    {
        List<Demon> demonsToDestroy = new List<Demon>();
        foreach (Demon demon in demons)
        {
            if (!demon.InPlay)
            {
                demonsToDestroy.Add(demon);
            }
        }

        foreach (Demon demon in demonsToDestroy)
        {
            demon.Destroy();
            RemoveDemon(demon);
        }

        List<Sheep> sheepToDestroy = new List<Sheep>();
        foreach (Sheep sheepInd in sheep)
        {
            if (!sheepInd.InPlay)
            {
                sheepToDestroy.Add(sheepInd);
            }
        }

        foreach (Sheep sheepInd in sheepToDestroy)
        {
            sheepInd.Destroy();
            RemoveSheep(sheepInd);
        }
    }
}
