using Godot;
using System;

public class LoseScreen : Control
{
	[Export] PackedScene gameScene;
	[Export] PackedScene menuScene;
	[Export] NodePath animationPlayerPath;

	public override void _Ready()
	{
		GetNode<Button>("RetryButton").Connect("pressed", this, nameof(OnRetryButtonPressed));
		GetNode<Button>("MenuButton").Connect("pressed", this, nameof(OnMenuButtonPressed));
		
		AnimationPlayer anim = GetNode<AnimationPlayer>(animationPlayerPath);
		anim.Play("frolic_over_fence-loop");
		GD.Print("CURRENT:", anim.CurrentAnimation);
		GD.Print(anim.GetAnimationList());
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
}
