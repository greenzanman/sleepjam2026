using Godot;
using System;

public class MenuScene : Control
{
    [Export] PackedScene gameScene;
    
    [Export] NodePath audioHoverPath;
    private AudioStreamPlayer audioHover;
    
    public override void _Ready()
    {
        GetNode<Button>("PlayButton").Connect("pressed", this, nameof(OnPlayButtonPressed));
        GetNode<Button>("PlayButton").Connect("mouse_entered", this, nameof(OnButtonHovered));
        
        audioHover = GetNode<AudioStreamPlayer>(audioHoverPath);
    }
    
    private void OnPlayButtonPressed()
    {
        GetTree().ChangeSceneTo(gameScene);
    }
    
    private void OnButtonHovered() 
    {
        audioHover.Stop();
        audioHover.Play();
    }
}
