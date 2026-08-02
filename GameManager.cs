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

    public static bool DebugDraw = true;

    private Player player;

    // Generic Game Settings
    private float timeDilation = 1;
    private bool isPaused = false; 

    // Sleep settings
    private GameState gameState = GameState.Awake;
    // Increments whenever a sheep jumps
    private float sleepCount = 0;
    public const int MAX_SLEEPCOUNT = 8;

    // Sheep
    private HashSet<Sheep> sheep = new HashSet<Sheep>();

    public override void _Ready()
    {
        Instance = this;
        timeDilation = 1;
        isPaused = false;
    }

    public override void _Process(float delta)
    {
        ProcessSleep(delta);
        ProcessDebug(delta);
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

// MARK: Sleep stuff
    public static void IncrementSleepCount() { Instance.sleepCount++; }
    public static float GetSleepCount() { return Instance.sleepCount; }

// MARK: Process functions
    private void ProcessSleep(float delta)
    {
        if (gameState == GameState.Awake)
        {
            if (sleepCount >= 8)
            {
                gameState = GameState.Dreaming;
            }
        }
        else
        {
            sleepCount -= delta;
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

        if (Input.IsActionJustPressed("key_debugSwap"))
        {
            GD.Print("Incrementing sleep count");
            sleepCount += 2;
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
}
