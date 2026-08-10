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
    public static Node2D WorldRoot;

    public static bool DebugDraw = false;

    private bool inPlay = true;
    private Player player;
    private float gameTime;

    // Generic Game Settings
    private float timeDilation = 1;
    private bool isPaused = false; 
    private float gameOverTimer = 0;

    // Sleep settings
    private GameState gameState = GameState.Awake;
    // Increments whenever a sheep jumps
    private float sleepCount = 0;
    public const int MAX_SLEEPCOUNT = 16;
    private float wakeRate = 1; // Increases once there are no demons
    private bool cyclePaused = false;

    private int nightCount = 0;

    private bool transitioning = false;
    private float sleepTransitionTimer = 0;
    private const float sleepTransitionDurationClose = 1.5f;
    private const float sleepTransitionDurationOpen = 1.5f;
    

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
        nightCount = 0;

        // WorldRoot = GetTree().Root.GetNode<Node2D>("World");
    }

    public void ResetWorldRoot()
    {
        WorldRoot = GetTree().Root.GetNode<Node2D>("World");
        transitioning = true;
        sleepTransitionTimer = 0;
    }

    public override void _Process(float delta)
    {
        // Transitions
        if (transitioning)
        {
            if (sleepTransitionTimer > 0 && sleepTransitionTimer - delta <= 0)
            {
                SetGameState( gameState == GameState.Awake ? GameState.Dreaming : GameState.Awake);    
            }
            
            sleepTransitionTimer -= delta;

            if (sleepTransitionTimer <= -sleepTransitionDurationOpen)
                transitioning = false;
        }

        float trueDelta = delta * GetTimeDilation();

        gameTime += trueDelta;

        ProcessSleep(trueDelta);
        ProcessCleanup(trueDelta);
    }

// MARK: Getters and Setters
    public static void SetPlayer( Player inPlayer ) { Instance.player = inPlayer; }
    public static Vector2 GetPlayerWorldPosition() { return Instance.GetPlayerWorldPositionInternal(); }
    public Vector2 GetPlayerWorldPositionInternal() { 
        if (player == null || !inPlay)
        {
            GD.PrintErr("Attempted to get player position before it's set");
            return Vector2.Zero;
        }
        return player.Position;
    }

    public static bool GetPaused() { return Instance.isPaused; }

    public static float GetTimeDilation() { return Instance.InternalGetTimeDilation(); }
    public float InternalGetTimeDilation()
    {
        if (isPaused)
            return 0;

        // Slowdowns
        if (transitioning) {
            if (sleepTransitionTimer > 0)
            {
                return sleepTransitionTimer / sleepTransitionDurationClose;
            }
            else
            {
                return Mathf.Min( -sleepTransitionTimer / sleepTransitionDurationOpen, 1);
            }
        }

        return Instance.timeDilation;
    }

    public static float GetGameTime()
    {
        return Instance.gameTime;
    }

    public static int GetNightCount()
    {
        return Instance.nightCount;
    }

    public static void SetNightCount(int count)
    {
        Instance.gameOverTimer = 0;
        StatKeeper.ResetStats();
        Instance.nightCount = count;
    }
    
    public static GameState GetGameState() {return Instance.gameState;}
    public static void SetGameState( GameState newGameState ) { Instance.SetGameStateInternal( newGameState ); }
    private void SetGameStateInternal( GameState newGameState )
    {
        gameState = newGameState;
        // Do other stuff here. Send delegate alert to all nodes?
    }

    public static void AddSheep( Sheep newSheep ) { 
        Instance.sheep.Add(newSheep); 
    }
    public static void RemoveSheep( Sheep oldSheep ) { 
        Instance.sheep.Remove(oldSheep); 
    }
    public static ref readonly HashSet<Sheep> GetSheep() { return ref Instance.sheep;} 
    
    public static void AddDemon( Demon newDemon ) { 
        Instance.demons.Add(newDemon); 
    }
    
    public static void RemoveDemon( Demon oldDemon ) { 
        Instance.demons.Remove(oldDemon); 
    }
    public static ref readonly HashSet<Demon> GetDemons() { return ref Instance.demons;}

// MARK: Sleep stuff
    public static void IncrementSleepCount( int amount = 1) { 
        Instance.sleepCount = Math.Min(Instance.sleepCount + amount, MAX_SLEEPCOUNT); 
    }
    public static float GetSleepCount() { return Instance.sleepCount; }

    // Int is 0 - not transition, 1 - closing, 2 - opening
    public static (float, int) GetSleepTransitionRatio() { return Instance.InternalTransitionRatio(); }
    private (float, int) InternalTransitionRatio()
    {
        if (transitioning)
        {
            if (sleepTransitionTimer > 0)
            {
                return (1 - sleepTransitionTimer / sleepTransitionDurationClose, 1);
            }
            else
            {
                return (1 - Mathf.Min( -sleepTransitionTimer / sleepTransitionDurationOpen, 1), 2);
            }
        }
        else
        {
            return (0, 0);
        }
    }

// MARK: Process functions
    private void ProcessSleep(float delta)
    {
        if (gameState == GameState.Awake)
        {
            if (sleepCount >= MAX_SLEEPCOUNT && !transitioning)
            {
                transitioning = true;
                sleepTransitionTimer = sleepTransitionDurationClose;
                nightCount += 1;
                StatKeeper.NumSleepInstances += 1;
                wakeRate = 1;
            }
        }
        else
        {
            if (!transitioning)
                sleepCount -= delta * wakeRate;

            // Sleep decreases faster if nothing to do
            if (demons.Count == 0)
                wakeRate += delta * 2;
                
            if (sleepCount <= 0 && !transitioning)
            {
                transitioning = true;
                sleepTransitionTimer = sleepTransitionDurationClose;
                sleepCount = 0;
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
            IncrementSleepCount(2);
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

        if (Input.IsActionJustPressed("key_debugKill"))
        {
            // foreach (Demon demon in demons)
            // {
            //     demon.Die();
            // }
        
            foreach (Sheep sheep in sheep)
            {
                sheep.Bite();
            }
        }

        if (Input.IsActionJustPressed("key_debugTest"))
        {
            DemonSpawner.Instance.SpawnSleeper();
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
            StatKeeper.NumDemonsKilled += 1;
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
            StatKeeper.NumSheepDeaths += 1;
        }
        
        // Lose State
        if(sheep.Count <= 0 && GetTree().CurrentScene.Name == "World")
        {
            gameOverTimer += delta;
            if (gameOverTimer > 0.5f)
            {
                foreach (Demon d in demons)
                {
                    d.Destroy();
                }

                GD.Print("All sheep dead, game over");
                inPlay = false;

                // Temp fix
                Fence.Fences.Clear();
                sleepCount = 0;
                sheep.Clear();
                demons.Clear();

                GetTree().ChangeScene("res://UI/LoseScreen.tscn");
            }
        }
    }
}
