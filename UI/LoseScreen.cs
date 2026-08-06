using Godot;
using System;

public class LoseScreen : Control
{
	[Export] PackedScene gameScene;
	[Export] PackedScene menuScene;
	public override void _Ready()
	{
		GetNode<Button>("RetryButton").Connect("pressed", this, nameof(OnRetryButtonPressed));
		GetNode<Button>("MenuButton").Connect("pressed", this, nameof(OnMenuButtonPressed));
	}
	
	private void OnRetryButtonPressed()
	{
		GetTree().ChangeSceneTo(gameScene);
	}

	private void OnMenuButtonPressed()
	{
		GetTree().ChangeSceneTo(menuScene);
	}
}
