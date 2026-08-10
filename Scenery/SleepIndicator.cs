using Godot;
using System;
using System.Linq;

public class SleepIndicator : SleepNode
{
    Sprite lens;
    Sprite pupil;
    const int frames = 5;

    const int angleCount = 64;
    Vector2[] anglePoints = new Vector2[angleCount + 2];
    const float radius = 100;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        lens = GetNode<Sprite>("Lens");
        lens.Frame = 0;
        pupil = GetNode<Sprite>("Pupil");
        base._Ready();

        anglePoints[0] = Vector2.Zero;
        for (int i = 0; i <= angleCount; i++)
        {
            float angle = Mathf.Pi * 2 / angleCount * i - Mathf.Pi / 2;
            anglePoints[i + 1] = new Vector2(radius * -Mathf.Cos(angle), radius * Mathf.Sin(angle));
        }
    }
    // In process, so it's not affected by dilation
    public override void _Process(float delta)
    {
        base._Process(delta);
        
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
        (float transitionRatio, int transitionType) = GameManager.GetSleepTransitionRatio();
        bool shouldBeEmpty = transitionRatio == 0;

        int frame;
        if (transitionType == 2)
            frame = Math.Min(frames - 2, (int) ((frames - 1) * transitionRatio));
        else
            frame = Math.Min(frames - 1, (int) (frames * transitionRatio));

        pupil.Visible = shouldBeEmpty || (frame == 0);
        if (frame == 0 || shouldBeEmpty || GameManager.GetSleepCount() == 0)
        {
            frame += frames;
        }
        lens.Frame = frame;

        if (pupil.Visible)
        {
            float angleProportion = 1 - GameManager.GetSleepCount() / GameManager.MAX_SLEEPCOUNT;
            int points = (int) (angleProportion * angleCount);
            points = Math.Min(Math.Max(points, 0), angleCount);
            if (points == 64)
                DrawColoredPolygon(anglePoints.Skip(1).Take(points).ToArray(), currentGameState == GameState.Awake ? GameSettings.colorDark : GameSettings.colorLight);
            else if (points >= 1)
                DrawColoredPolygon(anglePoints.Take(points + 2).ToArray(), currentGameState == GameState.Awake ? GameSettings.colorDark : GameSettings.colorLight);
        }
        
    }

}
