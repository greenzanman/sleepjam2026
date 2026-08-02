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

    private Player player;

    // Generic Game Settings
    private float timeDilation = 1;
    private bool isPaused = false; 

    // Swimming settings
    private GameState gameState = GameState.Awake;
    private float depth = 0; // Position of all objects offset by 'depth' to simulate sinking
    private float sleepTimer = 0;

    // Enemy Settings
    private HashSet<FishBase> fishes = new HashSet<FishBase>();

    public override void _Ready()
    {
        Instance = this;
        timeDilation = 1;
        isPaused = false;
    }

    public override void _Process(float delta)
    {
        ProcessDebug(delta);
    }

// MARK: Getters and Setters
    public static void SetPlayer( Player inPlayer ) { Instance.player = inPlayer; }
    public static Vector2 GetPlayerWorldPosition() { return Instance.GetPlayerWorldPositionInternal(); }
    public Vector2 GetPlayerWorldPositionInternal() { 
        if (player == null)
        {
            GD.PrintErr("Attempted to get player position before it's set");
            return Vector2.Zero;
        }
        return player.Position + Vector2.Down * depth; 
    }

    public static void AddFish( FishBase inFish ) { Instance.fishes.Add(inFish); }
    public static void RemoveFish( FishBase outFish ) { Instance.fishes.Remove(outFish); }
    public static HashSet<FishBase> GetFishes() { return Instance.fishes; }

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

    public static float GetDepth() { return Instance.depth; }
    public static void UpdateDepth(float change) { Instance.depth += change; }

// MARK: Process functions

    private void ProcessDebug(float delta)
    {
        string pauseString = isPaused ? "\nPAUSED" : "";
        DebugManager.SetDebugText($"Depth: {depth:0.00}" + pauseString);

        if (Input.IsActionJustPressed("key_debugSwap"))
        {
            GD.Print("Swapping Game State");
            SetGameState( 1 - GetGameState() );
        }

        if (Input.IsActionJustPressed("key_pause"))
        {
            GD.Print("Toggling pause");
            isPaused = !isPaused;
        }
    }
}
