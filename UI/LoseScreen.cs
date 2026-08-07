using Godot;
using System;

public class LoseScreen : Control
{
    [Export] PackedScene gameScene;
    [Export] PackedScene menuScene;
    
    [Export] NodePath animationPlayerPath;
    [Export] NodePath retryButtonPath;
    [Export] NodePath menuButtonPath;
    [Export] NodePath statLabelPath;
    
    private Label statLabel;
    
    private int statsShown = 0;
    public override void _Ready()
    {
        GetNode<Button>(retryButtonPath).Connect("pressed", this, nameof(OnRetryButtonPressed));
        GetNode<Button>(menuButtonPath).Connect("pressed", this, nameof(OnMenuButtonPressed));
        
        statLabel = GetNode<Label>(statLabelPath);
        GetNode<AnimationPlayer>(animationPlayerPath).Play("frolic_over_fence-loop");
        // StatKeeper.PrintStats();
    }

    private void OnRetryButtonPressed()
    {
        GetTree().ChangeSceneTo(gameScene);
    }

    private void OnMenuButtonPressed()
    {
        GetTree().ChangeSceneTo(menuScene);
    }
    
    private void SetStatLabelText() 
    {
        
        switch (statsShown) 
        {
            case (0):
                statLabel.Text = $"Demons Killed: {StatKeeper.NumDemonsKilled}";
                break;
            case (1):
                statLabel.Text = $"Fence Jumps: {StatKeeper.NumFenceJumps}";
                break;
            case (2):
                statLabel.Text = $"Sheep Deaths: {StatKeeper.NumSheepDeaths}";
                break;
            case (3):
                statLabel.Text = $"Dreams: {StatKeeper.NumSleepInstances}";
                break;
            case (4):
                statLabel.Text = $"Sheep Purified: {StatKeeper.NumSheepPurified}";
                break;
            case (5):
                statLabel.Text = $"Barks: {StatKeeper.NumBarks}";
                break;
        }
        statsShown++;
    }
}
