using Godot;
using System;
using System.Collections.Generic;

public class Player : Node2D
{
    private GameState mostRecentGamestate;

    private float awakeVelocity;
    private Vector2 lastDreamInput;
    private Vector2 dreamVelocity;

    private const float AWAKE_ACCELERATION = 300;
    private const float AWAKE_MAXSPEED = 60;
    private const float AWAKE_SINKRATE = 60;

    private const float DREAM_ACCELERATION = 1000;
    private const float DREAM_MAXSPEED = 250;

    private PackedScene hitboxPrefab;

    public override void _Ready()
    {
        GameManager.SetPlayer(this);
        UpdateGameState(GameManager.GetGameState());

        hitboxPrefab = GD.Load<PackedScene>("res://Player/AttackHitbox.tscn");
    }

    public override void _Process(float delta)
    {
        float timeDelta = delta * GameManager.GetTimeDilation();

        GameState currentGameState = GameManager.GetGameState();
        if (currentGameState != mostRecentGamestate)
            UpdateGameState(currentGameState);

        switch ( currentGameState )
        {
            case GameState.Awake:
            ProcessAwake(timeDelta);
            break;
            case GameState.Dreaming:
            ProcessDreaming(timeDelta);
            break;
        }

        // Position snapping
        float borderPadding = 32;
        if (Position.x >= GameSettings.ScreenWidth - borderPadding)
        {
            Position = new Vector2(GameSettings.ScreenWidth - borderPadding, Position.y);
            dreamVelocity.x = Mathf.Min(dreamVelocity.x, 0);
            awakeVelocity = Mathf.Min(awakeVelocity, 0);
        }
        if (Position.x <= borderPadding)
        {
            Position = new Vector2(borderPadding, Position.y);
            dreamVelocity.x = Mathf.Max(dreamVelocity.x, 0);
            awakeVelocity = Mathf.Max(awakeVelocity, 0);
        }
    }

    private void ProcessAwake(float delta)
    {
        float movementInput = Input.GetAxis("key_left", "key_right");

        awakeVelocity = Utils.MoveTowards(awakeVelocity, movementInput * AWAKE_MAXSPEED, AWAKE_ACCELERATION * delta);
        
        // TODO: small dashes?

        Position += Vector2.Right * awakeVelocity * delta;
        GameManager.UpdateDepth(AWAKE_SINKRATE * delta);
    }

    private void ProcessDreaming(float delta)
    {
        Vector2 movementInput = new Vector2(Input.GetAxis("key_left", "key_right"),
            Input.GetAxis("key_up", "key_down"));

        if (movementInput != Vector2.Zero)
        {
            movementInput = movementInput.Normalized();
            lastDreamInput = movementInput;
        }

        dreamVelocity = Utils.MoveTowards(dreamVelocity, movementInput * DREAM_MAXSPEED, DREAM_ACCELERATION * delta);

        Position += Vector2.Right * dreamVelocity.x * delta;
        GameManager.UpdateDepth(dreamVelocity.y * delta);
    
        // TODO: Attacking
        if (Input.IsActionJustPressed("key_action"))
        {
            AttackHitbox hitbox = hitboxPrefab.Instance<AttackHitbox>();
            GetTree().Root.AddChild(hitbox);
            hitbox.Position = Position + lastDreamInput * 32;

            // Temp damage calculations
            HashSet<FishBase> hitFishes = new HashSet<FishBase>();
            foreach( FishBase fish in GameManager.GetFishes())
            {
                if (fish.Position.x < hitbox.Position.x + 48 && 
                fish.Position.x > hitbox.Position.x - 48 && 
                fish.Position.y < hitbox.Position.y + 48 && 
                fish.Position.y > hitbox.Position.y - 48)
                {
                    hitFishes.Add(fish);
                }
            }
            foreach ( FishBase fish in hitFishes)
                fish.TakeDamage(1);
        }
    }

    private void UpdateGameState(GameState newGameState)
    {
        mostRecentGamestate = newGameState;

        if (newGameState == GameState.Awake)
        {
            Modulate = GameSettings.colorLight;
        
            awakeVelocity = 0;
        }
        else
        {
            Modulate = GameSettings.colorDark;

            dreamVelocity = Vector2.Zero;
            lastDreamInput = Vector2.Right;
        }
    }
}
