using Godot;
using System;
using System.Collections.Generic;

public class Player : SleepNode
{

    private Vector2 velocity;
    private const float ACCELERATION = 5000;
    private const float MAXSPEED = 400;

    Node2D sprite;

    // Awake state
    const float barkCooldown = 0.5f;
    const float barkInputBufferWindow = 0.12f;
    const float fakeBarkBuffer = 0.05f; // Bar is slightly slower so it 'feels' better

    float barkTimer = 0;
    float barkInputBufferTimer = 0;

    float misTimer = 0;
    const float biteRange = 100;
    const float misDisplayDuration = 0.5f;
    const int angleCount = 64;
    Vector2[] anglePoints = new Vector2[angleCount + 1];

    public override void _Ready()
    {
        sprite = GetNode<Node2D>("Polygon2D");
        base._Ready();
        GameManager.SetPlayer(this);

        float radius = biteRange - 20;
        for (int i = 0; i <= angleCount; i++)
        {
            float angle = Mathf.Pi * 2 / angleCount * i;
            anglePoints[i] = new Vector2(radius * -Mathf.Cos(angle), radius * Mathf.Sin(angle));
        }
    }

    protected override void Process(float delta)
    {
        ProcessMovement(delta);

        Update(); // TODO: Build this into sprite or something
    }

    protected override void SetModulate(bool awake)
    {
        if (awake)
        {
            sprite.Modulate = GameSettings.colorLight;
        }
        else
        {
            sprite.Modulate = GameSettings.colorDark;
        }
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
        barkTimer = Mathf.Max(-10, barkTimer - delta);

        barkInputBufferTimer = Mathf.Max(0, barkInputBufferTimer - delta);
        if (Input.IsActionJustPressed("key_action"))
            barkInputBufferTimer = barkInputBufferWindow;

        if (barkTimer <= 0 && barkInputBufferTimer > 0)
        {
            foreach (Sheep sheep in GameManager.GetSheep())
            {
                sheep.Bark(Position);
            }

            barkTimer = barkCooldown;
            barkInputBufferTimer = 0;
            StatKeeper.NumBarks += 1;
        }
    }

    protected override void UpdateGameState(GameState newGameState)
    {
        base.UpdateGameState(newGameState);
 
        barkTimer = 0;
        misTimer = 0;
    }

    protected override void ProcessDreaming(float delta)
    {
        Demon closestDemon = null;
        float closestDistance = biteRange;

        bool hurtDemonInRange = false;
        if (Input.IsActionJustPressed("key_action"))
        {
            // TODO: Improve performance of all these iteration distance checks
            foreach (Demon demon in GameManager.GetDemons())
            {
                if (!demon.Bitable)
                    continue;
                    
                float distance = (demon.Position - Position).Length();

                // Not displaying indicator if demon is just in hitstate
                if (distance <= biteRange && (!demon.IsAlive || demon.IsHurt()))
                {
                    hurtDemonInRange = true;
                    continue;
                }

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDemon = demon;
                }
            }

            if (closestDemon != null)
            {
                closestDemon.Bite();
            }
            else if (!hurtDemonInRange)
            {
                misTimer = misDisplayDuration;
            }
        }

        misTimer = Mathf.Max(misTimer - delta, 0);
    }

    public override void _Draw()
    {
        base._Draw();
        if (currentGameState == GameState.Awake)
        {
            float fill = 60 * Mathf.Max(barkTimer + fakeBarkBuffer, 0) / (barkCooldown + fakeBarkBuffer);
            DrawLine(new Vector2(-30, -30), new Vector2(-30 + fill, -30), GameSettings.colorLight, 5);
        }
        else
        {
            if (misTimer > 0 && (int)(misTimer * GameSettings.FlashRate) % 2 == 0)
            {
                DrawMultiline(anglePoints, GameSettings.colorDark,
                    3);
            }
        }
    }
}
