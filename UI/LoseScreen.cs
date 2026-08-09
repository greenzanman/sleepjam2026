using Godot;
using System;

public class LoseScreen : Control
{
	[Export] PackedScene gameScene;
	[Export] PackedScene menuScene;
	
	[Export] NodePath animationPlayerPath;
	[Export] NodePath retryButtonPath;
	[Export] NodePath menuButtonPath;
	[Export] NodePath tempStatLabelPath;
	[Export] NodePath permStatLabelPath;
	
	private Label tempStatLabel;
	private Label permStatLabel;
	
	private int statsShown = 0;
	public override void _Ready()
	{
		GetNode<Button>(retryButtonPath).Connect("pressed", this, nameof(OnRetryButtonPressed));
		GetNode<Button>(menuButtonPath).Connect("pressed", this, nameof(OnMenuButtonPressed));
		
		tempStatLabel = GetNode<Label>(tempStatLabelPath);
		permStatLabel = GetNode<Label>(permStatLabelPath);
		tempStatLabel.Text = "";
		permStatLabel.Text = "";
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
	
	private void SetLabelText() 
	{
		
		switch (statsShown) 
		{
			case (0):
				tempStatLabel.Text = $"Demons Killed: {StatKeeper.NumDemonsKilled}";
				permStatLabel.Text += $"Demons Killed: {StatKeeper.NumDemonsKilled}\n";
				break;
			case (1):
				tempStatLabel.Text = $"Fence Jumps: {StatKeeper.NumFenceJumps}";
				permStatLabel.Text += $"Fence Jumps: {StatKeeper.NumFenceJumps}\n";
				break;
			case (2):
				tempStatLabel.Text = $"Sheep Deaths: {StatKeeper.NumSheepDeaths}";
				permStatLabel.Text += $"Sheep Deaths: {StatKeeper.NumSheepDeaths}\n";
				break;
			case (3):
				tempStatLabel.Text = $"Dreams: {StatKeeper.NumSleepInstances}";
				permStatLabel.Text += $"Dreams: {StatKeeper.NumSleepInstances}\n";
				break; 
			case (4):
				tempStatLabel.Text = $"Sheep Purified: {StatKeeper.NumSheepPurified}";
				permStatLabel.Text += $"Sheep Purified: {StatKeeper.NumSheepPurified}\n";
				break;
			case (5):
				tempStatLabel.Text = $"Barks: {StatKeeper.NumBarks}";
				permStatLabel.Text += $"Barks: {StatKeeper.NumBarks}\n";
				break;
			case (6):
				tempStatLabel.Text = "Art and Programming:\n Greenzanman";
				break;
			case (7):
				tempStatLabel.Text = "Concepts and Programming:\n ChimeraLC";
				break;
			case (8):
				tempStatLabel.Text = "Concepts and Programming:\n LuckyLuciano314";
				break;
			case (9):
				tempStatLabel.Text = "Developed on:\n Godot";
				break;
			default:
				tempStatLabel.Text = "Thanks For Playing! :)";
				break;
		}
		statsShown++;
	}
}
