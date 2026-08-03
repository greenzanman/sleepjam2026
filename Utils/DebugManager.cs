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

    public static void SetDebugText(string text)
    {
        if (Instance != null)
        {
            Instance.debugText.Text = text;
        }
    }
}
