using Godot;

// Generic class for any object that has different awake/dreaming behavior
public abstract class SleepNode : Node2D
{

    private GameState mostRecentGamestate;

    public override void _Ready()
    {
        mostRecentGamestate = GameManager.GetGameState();
        UpdateGameState(GameManager.GetGameState());
    }

    public override void _Process(float delta)
    {
        float trueDelta = delta * GameManager.GetTimeDilation();

        GameState currentGameState = GameManager.GetGameState();
        if (currentGameState != mostRecentGamestate)
        {
            mostRecentGamestate = currentGameState;
            UpdateGameState(currentGameState);
        }

        Process(trueDelta);

        switch ( currentGameState )
        {
            case GameState.Awake:
            ProcessAwake(trueDelta);
            break;
            case GameState.Dreaming:
            ProcessDreaming(trueDelta);
            break;
        }
    }


    // 'True' process function that accounts for time delta; Called before Awake/Dreaming
    protected virtual void Process(float delta) {}
    // Process function called while awake
    protected virtual void ProcessAwake(float delta) {}
    // Process function called while dreaming
    protected virtual void ProcessDreaming(float delta) {}
    // Called whenever gamestate changes
    protected virtual void UpdateGameState(GameState newGameState)
    {
        if (newGameState == GameState.Awake)
        {
            Modulate = GameSettings.colorLight;
        }
        else
        {
            Modulate = GameSettings.colorDark;
        }
    }

    // Helper functions
    protected GameState GetGameState() { return mostRecentGamestate; }
}
