using Godot;
using System;

public class SleepIndicator : SleepNode
{
    Sprite lens;
    Sprite pupil;
    const int frames = 5;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        lens = GetNode<Sprite>("Lens");
        lens.Frame = 0;
        pupil = GetNode<Sprite>("Pupil");
        base._Ready();
    }
    // In process, so it's not affected by dilation
    public override void _Process(float delta)
    {
        base._Process(delta);
        (float transitionRatio, int transitionType) = GameManager.GetSleepTransitionRatio();
        bool shouldBeEmpty = transitionRatio == 0;

        int frame;
        if (transitionType == 2)
            frame = Math.Max(0, Math.Min(frames - 2, (int) ((frames - 1) * transitionRatio)));
        else
            frame = Math.Max(0, Math.Min(frames - 1, (int) (frames * transitionRatio)));

        pupil.Visible = shouldBeEmpty || (frame == 0);
        if (frame == 0 || shouldBeEmpty || GameManager.GetSleepCount() == 0)
        {
            frame += frames;
        }
        lens.Frame = frame;
        
        Update();
    }

    protected override void UpdateGameState(GameState newGameState)
    {
        if (newGameState == GameState.Awake)
        {
            lens.Modulate = GameSettings.colorLight;
            pupil.Modulate = GameSettings.colorLight;
        }
        else
        {
            lens.Modulate = GameSettings.colorDark;  
            pupil.Modulate = GameSettings.colorDark; 
        }
    }


    public override void _Draw()
    {
        base._Draw();
        // Covor hidden parts of eye
        DrawArc(Vector2.Zero, 40, Mathf.Pi * 2 * GameManager.GetSleepCount() / GameManager.MAX_SLEEPCOUNT, 
           Mathf.Pi * 2, 64, 
           currentGameState == GameState.Awake ? GameSettings.colorDark : GameSettings.colorLight, 80);
    }

}
