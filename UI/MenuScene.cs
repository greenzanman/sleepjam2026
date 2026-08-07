using Godot;
using System;

public class MenuScene : Control
{
	[Export] PackedScene gameScene;
	public override void _Ready()
	{
		GetNode<Button>("PlayButton").Connect("pressed", this, nameof(OnPlayButtonPressed));
	}
	
	private void OnPlayButtonPressed()
	{
		GetTree().ChangeSceneTo(gameScene);
	}
}
