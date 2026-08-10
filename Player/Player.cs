using Godot;
using System;
using System.Collections.Generic;

public class Player : SleepNode
{
    private AudioStreamPlayer audioBark;
    
    private Vector2 velocity;
    private const float ACCELERATION = 5000;
    private const float MAXSPEED = 400;

    Sprite spriteFront;
    Sprite spriteBack;

    // Awake state
    const float overallPadding = 10;
    const float barkCooldown = 0.5f;
    const float barkInputBufferWindow = 0.12f;
    const float fakeBarkBuffer = 0.05f; // Bar is slightly slower so it 'feels' better

    float barkTimer = 0;
    float barkInputBufferTimer = 0;

    float misTimer = 0;
    const float biteRange = 100;
    const float misDisplayDuration = 0.5f;
    const int angleCount = 64;
    Random random = new Random();
    Vector2[] anglePoints = new Vector2[angleCount + 1];

    public override void _Ready()
    {
        spriteFront = GetNode<Sprite>("SpriteFront");
        spriteBack = GetNode<Sprite>("SpriteBack");
        base._Ready();
        GameManager.SetPlayer(this);
        
        audioBark = GetNode<AudioStreamPlayer>("AudioStreamPlayer");

        float radius = biteRange - 20;
        for (int i = 0; i <= angleCount; i++)
        {
            float angle = Mathf.Pi * 2 / angleCount * i;
            anglePoints[i] = new Vector2(radius * -Mathf.Cos(angle), radius * Mathf.Sin(angle));
        }

    }

    const int animationRate = 4;
    protected override void Process(float delta)
    {
        ProcessMovement(delta);

        // Animation
        int frame = 0;
        
        float currentSpeed = velocity.Length();
        if (currentSpeed > 5)
        {
            frame = ((int) (GameManager.GetGameTime() * animationRate * 2)) % 5 + 8;
            if (barkTimer > 0)
                frame += 8;
        }
        else
        {
            frame = ((int) (GameManager.GetGameTime() * animationRate)) % 3;
            if (barkTimer > 0)
                frame += 4;
        }

        spriteFront.Frame = frame;
        spriteBack.Frame = frame;

        Update(); // TODO: Build this into sprite or something
    }

    protected override void SetModulate(bool awake)
    {
        if (awake)
        {
            spriteFront.Modulate = GameSettings.colorLight;
            spriteBack.Modulate = GameSettings.colorDark;
        }
        else
        {
            spriteFront.Modulate = GameSettings.colorDark;
            spriteBack.Modulate = GameSettings.colorLight;
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
        Vector2 newPosition = Position + velocity * delta;
        if (newPosition.x > GameSettings.ScreenWidth - overallPadding)
            newPosition.x = GameSettings.ScreenWidth - overallPadding;
        if (newPosition.x < overallPadding)
            newPosition.x = overallPadding;
        if (newPosition.y > GameSettings.ScreenHeight - overallPadding)
            newPosition.y = GameSettings.ScreenHeight - overallPadding;
        if (newPosition.y < overallPadding)
            newPosition.y = overallPadding;

        Position = newPosition;

        spriteFront.FlipH = velocity.x < 0;
        spriteBack.FlipH = velocity.x < 0;
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
            audioBark.Stop();
            audioBark.PitchScale = 0.8f + (float) random.NextDouble() * 0.3f;
            audioBark.Play(0.1f);
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
        // Reuse barktimer for sprite visuals
        barkTimer = Mathf.Max(-10, barkTimer - delta);
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
                barkTimer = barkCooldown;
                closestDemon.Bite();
                audioBark.Stop();
                audioBark.PitchScale = 0.6f + (float) random.NextDouble() * 0.3f;
                audioBark.Play(0.1f);
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
