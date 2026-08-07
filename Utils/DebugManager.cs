using Godot;
using System;

public class DebugManager : Control
{
    
    public static DebugManager Instance { get; private set; }
    private RichTextLabel debugText;

    public override void _Ready()
    {
        Instance = this;
        debugText = GetNode<RichTextLabel>("DebugText");
    }
    
     public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void SetDebugText(string text)
    {
        if (Instance != null && IsInstanceValid(Instance) && Instance.debugText != null)
        {
            Instance.debugText.Text = text;
        }
    }
}
