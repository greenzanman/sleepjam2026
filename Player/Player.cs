using Godot;
using System;
using System.Collections.Generic;

public class Player : SleepNode
{

    private Vector2 velocity;
    private const float ACCELERATION = 5000;
    private const float MAXSPEED = 400;

    public override void _Ready()
    {
        base._Ready();
        GameManager.SetPlayer(this);
    }

    protected override void Process(float delta)
    {
        ProcessMovement(delta);
    }

    private void ProcessMovement(float delta)
    {
        
        Vector2 movementInput = new Vector2(Input.GetAxis("key_left", "key_right"),
            Input.GetAxis("key_up", "key_down"));

        if (movementInput != Vector2.Zero)
        {
            movementInput = movementInput.Normalized();
        }

        velocity = Utils.MoveTowards(velocity, movementInput * MAXSPEED, ACCELERATION * delta);
        Position += velocity * delta;
        // TODO: Keep within boundaries

    }

    protected override void ProcessAwake(float delta)
    {
        if (Input.IsActionJustPressed("key_action"))
        {
            foreach (Sheep sheep in GameManager.GetSheep())
            {
                sheep.Bark(Position);
            }
        }
    }

    protected override void ProcessDreaming(float delta)
    {
        Demon closestDemon = null;
        float closestDistance = 100;
        if (Input.IsActionJustPressed("key_action"))
        {
            // TODO: Improve performance of all these iteration distance checks
            foreach (Demon demon in GameManager.GetDemons())
            {
                if (!demon.IsAlive)
                    continue;
                    
                float distance = (demon.Position - Position).Length();
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDemon = demon;
                }
            }
        }
        if (closestDemon != null)
        {
            closestDemon.Bite();
        }
    }
}
