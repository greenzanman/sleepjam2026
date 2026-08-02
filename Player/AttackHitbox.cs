using Godot;
using System;

public class AttackHitbox : Node2D
{

    private float lifespan = 0.1f;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    public override void _Process(float delta)
    {
        lifespan -= delta * GameManager.GetTimeDilation();
        if (lifespan < 0)
            QueueFree();
    }
}
