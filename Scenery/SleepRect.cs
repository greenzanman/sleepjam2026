using Godot;
using System;

public class SleepRect : ColorRect
{
    [Export]
    public bool isFlipped;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    public void SetState(GameState newGameState)
    {   
        if (newGameState == GameState.Awake)
        {
            Color = isFlipped ? GameSettings.colorLight : GameSettings.colorDark;
        }
        else
        {
            Color = isFlipped ? GameSettings.colorDark : GameSettings.colorLight;
        }
    }
}
