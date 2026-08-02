using Godot;
using System;

public class Background : Control
{
    private GameState mostRecentGamestate;
    private ColorRect backgroundRect;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        backgroundRect = GetNode<ColorRect>("BackgroundColor");
        UpdateGameState(GameManager.GetGameState());
    }

    public override void _Process(float delta)
    {
        GameState currentGameState = GameManager.GetGameState();
        if (currentGameState != mostRecentGamestate)
            UpdateGameState(currentGameState);
    }

    
    private void UpdateGameState(GameState newGameState)
    {
        mostRecentGamestate = newGameState;

        if (newGameState == GameState.Awake)
        {
            backgroundRect.Color = GameSettings.colorDark;
        }
        else
        {
            backgroundRect.Color = GameSettings.colorLight;
        }
    }
}
