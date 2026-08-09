using Godot;
using System;

public class TransitionAudio : Godot.AudioStreamPlayer
{
    [Export] private float awakePitch = 1.0f;
    [Export] private float asleepPitch = 0.5f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        PitchScale = awakePitch;
    }
    
    public override void _Process(float delta) 
    {
        var (ratio, transitionType) = GameManager.GetSleepTransitionRatio();

        if (transitionType == 1) // Closing
        {
            PitchScale = Mathf.Lerp(awakePitch, asleepPitch, ratio);
        }
        else if (transitionType == 2) // Opening
        {
            PitchScale = Mathf.Lerp(asleepPitch, awakePitch, ratio);
        }
        else
        {
            PitchScale = awakePitch;
        }
    }
}
