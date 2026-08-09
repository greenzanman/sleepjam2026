using Godot;
using System;

public class Background : Control
{
    private GameState mostRecentGamestate;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
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

        foreach (Control node in GetChildren())
        {
            if (node is SleepRect sleepRect)
                sleepRect.SetState(newGameState);
        }
    }
}
