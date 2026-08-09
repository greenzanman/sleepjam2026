using Godot;
using System;

public class Background : Control
{
    private GameState mostRecentGamestate;

    private ColorRect sleepBar;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        UpdateGameState(GameManager.GetGameState());
    
        sleepBar = GetNode<ColorRect>("SleepRect");
    }


    public override void _Process(float delta)
    {
        GameState currentGameState = GameManager.GetGameState();
        if (currentGameState != mostRecentGamestate)
            UpdateGameState(currentGameState);

        // Set size of sleep bar
        sleepBar.RectSize = new Vector2(
            1000 / GameManager.MAX_SLEEPCOUNT * GameManager.GetSleepCount(), 40);
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
