using Godot;
using System;

public class FishBase : Node2D
{
    protected Vector2 worldPosition;
    private GameState mostRecentGamestate;
    public override void _Ready()
    {
        UpdateGameState(GameManager.GetGameState());
        GameManager.AddFish(this);
    }

    public virtual void Initialize(Vector2 inWorldPosition, double randVal)
    {
        worldPosition = inWorldPosition;
        Position = worldPosition - Vector2.Down * GameManager.GetDepth();
    }

    public virtual void TakeDamage(int damageAmount) {}

    protected virtual void Destroy()
    {
        QueueFree();

        // TODO: Queue these to happen at the end so there aren't looping issues
        GameManager.RemoveFish(this);
    }

    public override void _Process(float delta)
    {
        float timeDelta = delta * GameManager.GetTimeDilation();

        GameState currentGameState = GameManager.GetGameState();
        if (currentGameState != mostRecentGamestate)
            UpdateGameState(currentGameState);

        switch ( GameManager.GetGameState() )
        {
            case GameState.Awake:
            ProcessAwake(timeDelta);
            break;
            case GameState.Dreaming:
            ProcessDreaming(timeDelta);
            break;
        }

        // Match to screen
        Position = worldPosition - Vector2.Down * GameManager.GetDepth();

        if (Position.y < -128)
           Destroy();
    }

    // Swim back and forth
    protected virtual void ProcessAwake(float delta)
    {
    }

    protected virtual void ProcessDreaming(float delta)
    {
    }

    protected void UpdateGameState(GameState newGameState)
    {
        mostRecentGamestate = newGameState;

        if (newGameState == GameState.Awake)
        {
            Modulate = GameSettings.colorLight;

        }
        else
        {
            Modulate = GameSettings.colorDark;
        }
    }
}