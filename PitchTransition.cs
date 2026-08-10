using Godot;
using System;

public class PitchTransition : AudioStreamPlayer
{
    [Export] private float awakePitch = 1.0f;
    [Export] private float asleepPitch = 0.5f;

    private float currentPitch;
    private float targetPitch;

    private bool wasTransitioning = false;

    public override void _Ready()
    {
        GameState startingState = GameManager.GetGameState();

        currentPitch = startingState == GameState.Awake
            ? awakePitch
            : asleepPitch;

        targetPitch = currentPitch;
        PitchScale = currentPitch;
    }

    public override void _Process(float delta)
    {
        var (ratio, transitionType) = GameManager.GetSleepTransitionRatio();

        // A new transition has started.
        if (transitionType == 1 && !wasTransitioning)
        {
            // Alternate between awake and asleep pitch.
            targetPitch = targetPitch == awakePitch
                ? asleepPitch
                : awakePitch;
        }

        wasTransitioning = transitionType != 0;

        if (transitionType == 1) // Eye closing
        {
            PitchScale = Mathf.Lerp(currentPitch, targetPitch, ratio);
        }
        else if (transitionType == 2) // Eye opening
        {
            // Stay at the target pitch while the eye opens.
            PitchScale = targetPitch;
        }
        else
        {
            // No transition.
            PitchScale = targetPitch;
        }

        // Once the transition is completely finished,
        // the target becomes our new current pitch.
        if (transitionType == 0)
        {
            currentPitch = targetPitch;
        }
    }
}
