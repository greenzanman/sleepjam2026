using Godot;
using System;

public class Sheep : SleepNode
{
    AudioStreamPlayer audioBaah;
    AudioStreamPlayer audioJump;
    
    private float deathVolume = -25.0f;
    private enum SheepState
    {
        Idle,
        Wandering,
        Fleeing
    }

    private SheepState state;

    public bool IsAlive = true;
    public bool InPlay = true;
    

    // Being cursed
    public bool cursed = false; // Will not be attacked by demons
    private Node2D cursedIndicator;
    private Node2D cursedIndicatorBack;

    // Rebellious
    public bool rebellious = false;
    private float rebellionTimer = 0;
    private Node2D rebelliousIndicator;
    private Node2D rebelliousIndicatorBack;
    private float rebellionRange = 150;
    private const float rebelliousSpeed = 75;
    private double rebellionChance = 0.5; // Chance sheep in range rebells
    
    private Random random = new Random();
    
    // All movement
    // sprites
    private Node2D spritePosition;
    private Sprite sheepSpriteBack;
    private Sprite sheepSpriteFront;
    private AnimationPlayer animPlayer;
    private bool onFence = false;
    private const float fenceHopHeight = 12f;
    private const float fenceHopRiseRate = 70f;
    private const float fenceHopFallRate = 100f;
    
    private static PackedScene sparkleScene = GD.Load<PackedScene>("res://Sheep/Sparkle.tscn");

    float stateTimer = 0;
    Vector2 stateDirection;
    float stateSpeed;
    Vector2 currentVelocity = Vector2.Zero;

    const float movementAcceleration = 40f;
    const float movementDeceleration = 6f;

    // How close it can get to any edge
    float overallPadding = 20;

    // Fleeing
    const float fleeSpeed = 140;
    const float fleeSettleSpeed = 10;
    const int fleeCoolSubradius = 10;
    const float maxFleeDuration = 1.5f;
    const float minFleeDuration = 0.4f;
    const float fleeDistance = 250;
    float fleeDuration = 0;

    // Bark consecutive speedup
    // - more impulse on sheep if bark again on it
    float currentFleeTopSpeed = fleeSpeed;  // this will be flee speed after modifed by combo
    float currentFleeAcceleration = movementAcceleration;
    const float barkComboWindow = 1.0f;
    const float barkComboSpeedMultiplier = 1.1f;
    const float barkComboAccelerationMultiplier = 1.2f;
    const float barkComboImpulseFactor = 0.45f;
    float barkComboTimer = 0;
    int barkChainCount = 0;

    // Idle
    float idleSpeed = 40;
    int idleTicksSpent = 0;
    const int edgePadding = 50;
    // How far from fence idle points must at least be
    const int fencePadding = 50;

    // Wandering
    Vector2 wanderPoint;
    float wanderSpeed = 50;
    int wanderRange = 100;

    // Dreaming
    float hurtTimer = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        spritePosition = GetNode<Node2D>("SpritePosition");
        sheepSpriteFront = GetNode<Sprite>("SpritePosition/SpriteFront");
        sheepSpriteBack = GetNode<Sprite>("SpritePosition/SpriteBack");
        animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        cursedIndicator = GetNode<Node2D>("SpritePosition/CursedIndicator");
        rebelliousIndicator = GetNode<Node2D>("SpritePosition/RebelliousIndicator");

        cursedIndicatorBack = GetNode<Node2D>("SpritePosition/CursedIndicatorBack");
        rebelliousIndicatorBack = GetNode<Node2D>("SpritePosition/RebelliousIndicatorBack");
        base._Ready();
        GameManager.AddSheep(this);
        EnterNewState(SheepState.Idle);
    
        IsAlive = true;
        
        audioBaah = GetNode<AudioStreamPlayer>("SheepSoundPlayer");
        audioJump = GetNode<AudioStreamPlayer>("JumpSoundPlayer");
    }

    public void Bark( Vector2 position)
    {
        Vector2 potentialFleeDirection = Position - position;
        float fleeLength = potentialFleeDirection.Length();
        if (fleeLength < fleeDistance)
        {
            if (barkComboTimer > 0)
            {
                barkChainCount = barkChainCount + 1;
            }
            else
            {
                barkChainCount = 1;
            }
            barkComboTimer = barkComboWindow;

            bool isComboBark = barkChainCount >= 2;
            if (isComboBark)
            {
                currentFleeTopSpeed = fleeSpeed * barkComboSpeedMultiplier;
                //currentFleeAcceleration = movementAcceleration * barkComboAccelerationMultiplier;
            }
            else
            {
                // base movement if not combo
                currentFleeTopSpeed = fleeSpeed;
                //currentFleeAcceleration = movementAcceleration;
            }

            float newStateTimer = maxFleeDuration - (maxFleeDuration - minFleeDuration) * fleeLength / fleeDistance;
            // Add onto previous state if already fleeing (so a far bark doesn't cancel out a near one)
            if (state == SheepState.Fleeing)
                newStateTimer += stateTimer / 2;
            
            stateTimer = newStateTimer;
            fleeDuration = newStateTimer;

            stateDirection = Mathf.IsZeroApprox(fleeLength) ? Vector2.Left : potentialFleeDirection / fleeLength;
            EnterNewState(SheepState.Fleeing);

            // if (isComboBark)
            // {
            //     currentVelocity += stateDirection * (currentFleeTopSpeed * barkComboImpulseFactor);
            //     float maxComboVelocity = currentFleeTopSpeed * 1.35f;
            //     if (currentVelocity.LengthSquared() > maxComboVelocity * maxComboVelocity)
            //         currentVelocity = currentVelocity.Normalized() * maxComboVelocity;
            // }
        }
    }

    public virtual void Bite()
    {
        if (IsAlive)
        {
            hurtTimer = 0.25f;
            IsAlive = false;
            animPlayer.Play("die");
            PlayDeathSound();
        }
    }
    
    private void PlayDeathSound() 
    {
        AudioStreamPlayer deathSound = new AudioStreamPlayer();
        deathSound.Stream = GD.Load<AudioStream>("res://Sounds/u_b32baquv5u-8-bit-explosion-11-340459.mp3");
        deathSound.VolumeDb = deathVolume;
        deathSound.PitchScale = 0.8f;
        GameManager.WorldRoot.AddChild(deathSound);
        deathSound.Play();
        deathSound.Connect("finished", deathSound, "queue_free");
    }

    public virtual void Destroy()
    {
        QueueFree();
    }


    public bool InPen()
    {
        return Position.x > GameSettings.PenLeft && Position.x < GameSettings.PenRight &&
            Position.y > GameSettings.PenTop && Position.y < GameSettings.PenBottom;
    }

    protected override void SetModulate(bool awake)
    {
        if (awake)
        {
            sheepSpriteFront.Modulate = GameSettings.colorLight;
            sheepSpriteBack.Modulate = GameSettings.colorDark;
            cursedIndicator.Modulate = GameSettings.colorLight;
            rebelliousIndicator.Modulate = GameSettings.colorLight;
            cursedIndicatorBack.Modulate = GameSettings.colorDark;
            rebelliousIndicatorBack.Modulate = GameSettings.colorDark;
        }
        else
        {
            sheepSpriteFront.Modulate = GameSettings.colorDark;
            sheepSpriteBack.Modulate = GameSettings.colorLight;
            cursedIndicator.Modulate = GameSettings.colorDark;
            rebelliousIndicator.Modulate = GameSettings.colorDark;
            cursedIndicatorBack.Modulate = GameSettings.colorLight;
            rebelliousIndicatorBack.Modulate = GameSettings.colorLight;
        }
    }


    protected override void Process(float delta)
    {
        animPlayer.PlaybackSpeed = GameManager.GetTimeDilation();

        // Hurt display
        if (hurtTimer > 0)
        {
            hurtTimer -= delta;
            SetModulate( (int)(hurtTimer * GameSettings.FlashRate) % 2 == 1 );
            if (hurtTimer <= 0)
            {
                InPlay = false;
            }
        }

        // Indicators
        cursedIndicator.Visible = cursed;
        rebelliousIndicator.Visible = rebellious;

        if (rebellionTimer > 0)
        {
            rebellionTimer -= delta;
            rebelliousIndicator.Visible = (int) (rebellionTimer * GameSettings.FlashRate) % 2 == 1;
        }
    }

    protected override void UpdateGameState(GameState newGameState)
    {
        base.UpdateGameState(newGameState);
 
        if (newGameState == GameState.Dreaming)
        {
            rebellious = false;
            idleTicksSpent = 0; // Don't immediately wander upon waking up

            EnterNewState(InPen() ? SheepState.Idle : SheepState.Wandering);
        }
    }


    protected override void ProcessAwake(float delta)
    {
        barkComboTimer = Mathf.Max(0, barkComboTimer - delta);

        switch (state)
        {
            // Either move to another spot in the pen, stand still, or start wandering
            case SheepState.Idle:
                ProcessIdle(delta);
            break;
            // Flee from bark source
            case SheepState.Fleeing:
                audioBaah.Stop();
                audioBaah.PitchScale = 0.8f + (float) random.NextDouble() * 0.3f;
                audioBaah.Play();
                ProcessFleeing(delta);
            break;
            // Idle around a point outside the region
            case SheepState.Wandering:
                ProcessWandering(delta);
            break;
        }

        // Perform movement
        stateTimer -= delta;

        // i set up accel and decel.
        // - accel is for sheep being pushed like a bark
        // - decel for sheep settling down. so it keeps some inertia yknow
        // Vector2 targetVelocity = stateDirection * stateSpeed;
        // bool isAbouttaFlip = currentVelocity.Dot(targetVelocity) < 0;
        // bool isSpeedingUp = targetVelocity.LengthSquared() > currentVelocity.LengthSquared();
        // bool needsFastResponse = isAbouttaFlip || isSpeedingUp;
       
        // float currentAcceleration = state == SheepState.Fleeing ? currentFleeAcceleration : movementAcceleration;
        // float responseRate = needsFastResponse ? currentAcceleration : movementDeceleration;
        // float decay = Mathf.Exp(-responseRate * delta);
        // float velocityBlend = 1f - decay;
        // currentVelocity = currentVelocity.LinearInterpolate(targetVelocity, velocityBlend);

        // Keeping without bounds (maybe we don't want this; i.e. spook too far and they run off forever)

        Vector2 newPosition = Position + delta * stateDirection * stateSpeed;
        if ( state == SheepState.Fleeing && stateTimer < 1)
            newPosition = Position + delta * stateDirection * stateSpeed
                * (stateTimer / 1.25f + 0.2f);

        if (newPosition.x > GameSettings.ScreenWidth - overallPadding)
            newPosition.x = GameSettings.ScreenWidth - overallPadding;
        if (newPosition.x < overallPadding)
            newPosition.x = overallPadding;
        if (newPosition.y > GameSettings.ScreenHeight - overallPadding)
            newPosition.y = GameSettings.ScreenHeight - overallPadding;
        if (newPosition.y < overallPadding)
            newPosition.y = overallPadding;

        // leeway to stop velcity when blocked
        // if (Mathf.IsEqualApprox(newPosition.x, overallPadding) || Mathf.IsEqualApprox(newPosition.x, GameSettings.ScreenWidth - overallPadding))
        //     currentVelocity.x = 0;
        // if (Mathf.IsEqualApprox(newPosition.y, overallPadding) || Mathf.IsEqualApprox(newPosition.y, GameSettings.ScreenHeight - overallPadding))
        //     currentVelocity.y = 0;

        Position = newPosition;

        if (stateTimer <= 0)
        {
            // Find next movement goal
            FindNextMovement();
        }
        // Checking fence state
        bool nowOnFence = false;
        foreach (Fence fence in Fence.Fences)
        {
            if (fence.IsOver(Position))
            {
                nowOnFence = true;
                break;
            }
        }

        // Crossing fence
        if (onFence && !nowOnFence)
        {
            GameManager.IncrementSleepCount();
            StatKeeper.NumFenceJumps++;
        }
        if (!onFence && nowOnFence)
        {
            audioJump.Stop();
            audioJump.PitchScale = 0.95f + (float) random.NextDouble() * 0.1f;
            audioJump.Play();
            Node2D sparkle = sparkleScene.Instance<Node2D>();
            sparkle.Position = Position + new Vector2(random.Next(-40, 40),
                random.Next(-40, -30));
            GameManager.WorldRoot.AddChild(sparkle);
        }

        float targetOffset = nowOnFence ? fenceHopHeight : 0;
        float currentOffset = Utils.MoveTowards(spritePosition.Position.y, -targetOffset, 
            delta * (nowOnFence ? fenceHopRiseRate : fenceHopFallRate));
        spritePosition.Position = new Vector2(0, currentOffset);

        onFence = nowOnFence;
        
        UpdateAnimation();
    }

    private void EnterNewState( SheepState newState )
    {
        SheepState previousState = state;
        state = newState;
        switch (state)
        {
            case SheepState.Idle:

                stateSpeed = rebellious ? rebelliousSpeed : idleSpeed;

                FindNextMovement();
                break;

            case SheepState.Wandering:
                // Idle ticks only reset between unique periods of wandering
                idleTicksSpent = 0;
            
                // If in the pen, choose a side closer to where sheep currently is in
                int wallDistance = random.Next(wanderRange * 3 / 2, wanderRange * 2);
                
                if (InPen())
                {
                    int wanderSide = random.Next(2);
                    // Choose random wanter point on outside edges
                    if (wanderSide == 0)
                        wanderPoint = new Vector2(Position.x > GameSettings.ScreenWidth / 2 ? GameSettings.ScreenWidth - wallDistance : wallDistance, 
                        GameSettings.ScreenHeight / 2 + random.Next(0, GameSettings.ScreenHeight / 2 - wanderRange) * (Position.y > GameSettings.ScreenHeight / 2 ? 1 : -1));
                    if (wanderSide == 1)
                        wanderPoint = new Vector2(GameSettings.ScreenWidth / 2 + 
                            random.Next(0, GameSettings.ScreenWidth / 2 - wanderRange) * (Position.x > GameSettings.ScreenWidth / 2 ? 1 : -1), 
                            Position.y > GameSettings.ScreenHeight / 2 ? GameSettings.ScreenHeight - wallDistance : wallDistance);
                }
                else // Otherwise, run to nearest wall, to avoid crossing over the fence again. Prioritizing left and right since those are 'easier' to herd
                {
                    if ( Position.x < GameSettings.PenLeft)
                    {
                        wanderPoint = new Vector2(wallDistance, random.Next(wanderRange, GameSettings.ScreenHeight - wanderRange));
                    }
                    else if ( Position.x > GameSettings.PenRight)
                    {
                        wanderPoint = new Vector2(GameSettings.ScreenWidth - wallDistance, random.Next(wanderRange, GameSettings.ScreenHeight - wanderRange));
                    }
                    else if ( Position.y > GameSettings.PenBottom)
                    {
                        wanderPoint = new Vector2(random.Next(wanderRange, GameSettings.ScreenWidth - wanderRange), 
                            GameSettings.ScreenHeight - wallDistance);
                    }
                    else
                    {
                        wanderPoint = new Vector2(random.Next(wanderRange, GameSettings.ScreenWidth - wanderRange), wallDistance);   
                    }
                }

                stateSpeed = rebellious ? rebelliousSpeed : wanderSpeed;

                FindNextMovement();

                if (rebellious)
                {
                    foreach (Sheep sheep in GameManager.GetSheep())
                    {
                        if (sheep != this && sheep.IsAlive && sheep.state == SheepState.Idle)
                        {
                            if ((sheep.Position - Position).LengthSquared() < rebellionRange * rebellionRange &&
                                random.NextDouble() < rebellionChance)
                            {
                                sheep.rebellionTimer = 4;
                                sheep.EnterNewState(SheepState.Wandering);

                                // TODO: Maybe want them to rebell in similar directions?
                            }
                        }
                    }
                    // TODO: Get other sheep to wander
                }
                break;


            case SheepState.Fleeing:
                stateSpeed = currentFleeTopSpeed;
                break;
        }
    }

    private void FindNextMovement()
    {
        switch (state)
        {
            case SheepState.Idle:
                int chosenMode = random.Next(10);

                if (chosenMode <= (rebellious ? 4 : 3) || onFence) // Never stand still on fence
                {
                    // Find new idle location
                    Vector2 idleGoal = new Vector2(random.Next(GameSettings.PenLeft + edgePadding, GameSettings.PenRight - edgePadding),
                        random.Next(GameSettings.PenTop + edgePadding, GameSettings.PenBottom - edgePadding));

                    // Keep goal away from center fence
                    if (Mathf.Abs(idleGoal.x - GameSettings.FenceX) < fencePadding)
                    {
                        idleGoal.x += idleGoal.x > GameSettings.FenceX ? 
                            fencePadding :
                            - fencePadding;
                    }

                    float travelDistance = (idleGoal - Position).Length();
                    stateTimer = travelDistance / stateSpeed;

                    stateDirection = (idleGoal - Position) / travelDistance; 

                }
                else if (chosenMode <= 14 - idleTicksSpent && !rebellious) // rebellious sheep are never still
                {
                    // Wait for 1-4 seconds, and move again
                    stateTimer = random.Next(1, 2);
                    stateDirection = Vector2.Zero;
                }
                else // TODO: Make less likely if theres already a lot of wandering?
                {
                    EnterNewState(SheepState.Wandering);
                }

                idleTicksSpent++;
            break;

            case SheepState.Wandering:            
                // Rebellious sheep sometimes go back for more sheep
                if (rebellious && random.Next(3) == 0)
                {
                    EnterNewState(SheepState.Idle);
                }
                else // Otherwise, wander around wanter point
                {
                    Vector2 wanderGoal = wanderPoint + new Vector2(random.Next(-wanderRange, wanderRange), random.Next(-wanderRange, wanderRange));
                    float wanderDistance = (wanderGoal - Position).Length();
                    stateDirection = (wanderGoal - Position) / wanderDistance;
                    stateTimer = wanderDistance / stateSpeed;
                }
            break;

            case SheepState.Fleeing:
                if (stateSpeed <= (fleeSettleSpeed + 0.001f) || InPen())
                {
                    EnterNewState(InPen() ? SheepState.Idle : SheepState.Wandering);
                }
                else
                {
                    // Idle a bit at the end of fleeing if outside
                    int subradiusX = random.Next(-fleeCoolSubradius, fleeCoolSubradius);
                    subradiusX = subradiusX + 10 * Math.Sign(subradiusX);
                    int subradiusY = random.Next(-fleeCoolSubradius, fleeCoolSubradius);
                    subradiusY = subradiusY + 10 * Math.Sign(subradiusY);
                    Vector2 fleeOffset = new Vector2(subradiusX, subradiusY);
                    float fleeDistance = fleeOffset.Length();

                    stateDirection = fleeOffset / fleeDistance;
                    stateTimer = fleeDistance / fleeSettleSpeed;

                    stateSpeed = fleeSettleSpeed;
                }
            break;
        }
    }

    private void ProcessIdle(float delta)
    {
    }

    private void ProcessFleeing(float delta)
    {
        // Fleeing decrements faster in the pen
        stateTimer -= InPen() ? delta * 1f : 0;

        if (stateTimer <= 0)
            stateSpeed = fleeSettleSpeed;
    }

    private void ProcessWandering(float delta)
    {
    }

    protected override void ProcessDreaming(float delta)
    {
        UpdateAnimation();
    }

    // TODO: Make it so animation isn't checked every fram
    private void UpdateAnimation()
    {
        if (!IsAlive) return;  // dead anim handled in bite

        // always sleep in night  
        if (GetGameState() == GameState.Dreaming)
        {
            spritePosition.Position = Vector2.Zero;  // incase bro jumped
            if (animPlayer.CurrentAnimation != "sleep")
            {
                animPlayer.Play("sleep");
            }
            return;
        }

        if (onFence)
        {
            if (animPlayer.CurrentAnimation != "jump")
            {
                animPlayer.Play("jump");
            }
            return;
        }

        // need a certain amt of speed to do run
        if (stateSpeed > 0 && stateDirection != Vector2.Zero)
        {
            string targetAnim = state == SheepState.Fleeing ? "run_eyes" : "run";
            
            if (animPlayer.CurrentAnimation != targetAnim)
                animPlayer.Play(targetAnim);

            animPlayer.PlaybackSpeed *= Mathf.Min(stateSpeed / wanderSpeed, 1.5f);

            sheepSpriteFront.FlipH = stateDirection.x < 0; 
            sheepSpriteBack.FlipH = stateDirection.x < 0;

            return;
        }
        else
        {
            if (animPlayer.CurrentAnimation != "stand") {
                animPlayer.Play("stand");
            }
        }
    }
}
