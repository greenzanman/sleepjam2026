using Godot;
using System;
using System.Runtime.InteropServices;

public class Sheep : SleepNode
{
    private enum SheepState
    {
        Idle,
        Wandering,
        Fleeing
    }

    private SheepState state;

    public bool IsAlive = true;
    public bool InPlay = true;
    private Random random = new Random();
    
    // All movement
    private Area2D sheepArea;
    private Node2D sheepSprite;
    private bool onFence = false;

    float stateTimer = 0;
    Vector2 stateDirection;
    float stateSpeed;

    // Fleeing
    float fleeSpeed = 50;

    // Idle
    float idleSpeed = 40;
    int idleTicksSpent = 0;

    // Wandering
    Vector2 wanderPoint;
    float wanderSpeed = 80;
    int wanderRange = 100;

    // Dreaming
    float hurtTimer = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();

        GameManager.AddSheep(this);
        sheepSprite = GetNode<Node2D>("Sprite");
        sheepArea = GetNode<Area2D>("SheepArea");

        EnterNewState(SheepState.Idle);
    
        IsAlive = true;
    }

    public void Bark( Vector2 position)
    {
        Vector2 potentialFleeDirection = Position - position;
        float fleeLength = potentialFleeDirection.Length();
        if (fleeLength < 250)
        {
            stateTimer = 5 - fleeLength / 75;
            stateDirection = Mathf.IsZeroApprox(fleeLength) ? Vector2.Left : potentialFleeDirection / fleeLength;
            EnterNewState(SheepState.Fleeing);
        }
    }

    public virtual void Bite()
    {
        if (IsAlive)
        {
            hurtTimer = 0.25f;
            IsAlive = false;
        }
    }

    public virtual void Destroy()
    {
        QueueFree();
    }

    protected virtual bool InPen()
    {
        return Position.x > GameSettings.PenLeft && Position.x < GameSettings.PenRight &&
            Position.y > GameSettings.PenTop && Position.y < GameSettings.PenBottom;
    }

    protected override void Process(float delta)
    {
        // Hurt display
        if (hurtTimer > 0)
        {
            hurtTimer -= delta;
            Modulate = (int)(hurtTimer * 10) % 2 == 1 ? GameSettings.colorLight : GameSettings.colorDark;
            if (hurtTimer <= 0)
            {
                InPlay = false;
            }
        }
        // TODO: Avoidance

#if DEBUG
        Update();
#endif
    }

    protected override void ProcessAwake(float delta)
    {
        switch (state)
        {
            // Either move to another spot in the pen, stand still, or start wandering
            case SheepState.Idle:
                ProcessIdle(delta);
            break;
            // Flee from bark source
            case SheepState.Fleeing:
                ProcessFleeing(delta);
            break;
            // Idle around a point outside the region
            case SheepState.Wandering:
                ProcessWandering(delta);
            break;
        }

        // Perform movement
        stateTimer -= delta;
        Position += delta * stateDirection * stateSpeed;

        foreach (Area2D area in sheepArea.GetOverlappingAreas())
        {
            if (area.GetParent() is Sheep sheep)
            {
                Vector2 offset = sheep.Position - Position;
                bool inView = offset.Dot(stateDirection) > 0;

                if (inView)
                {
                    // Align direction slightly more
                    float distance = offset.Length();
                    // Max is 100?
                    if (distance < 40)
                        Position += delta * (40 - distance) * offset / distance * -1;
                }
            }
        }

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
            GameManager.IncrementSleepCount();

        onFence = nowOnFence;
        
        // Basic 'fence hopping' visual
        if (onFence)
        {
            sheepSprite.Position = Vector2.Up * 12;
        }
        else
        {
            sheepSprite.Position = Vector2.Zero;
        }
    }

    private void EnterNewState( SheepState newState )
    {
        state = newState;
        switch (state)
        {
            case SheepState.Idle:
                idleTicksSpent = 0;
                FindNextMovement();

                stateSpeed = idleSpeed;
            break;

            case SheepState.Wandering:              
                int wanderSide = random.Next(4);
                // Choose random wanter point on outside edges
                int wallDistance = random.Next(wanderRange, wanderRange * 2);
                if (wanderSide == 0)
                    wanderPoint = new Vector2(wallDistance, random.Next(wanderRange, GameSettings.ScreenHeight - wanderRange));
                if (wanderSide == 2)
                    wanderPoint = new Vector2(wallDistance, random.Next(GameSettings.ScreenHeight - wanderRange, GameSettings.ScreenHeight - wanderRange));
                if (wanderSide == 1)
                    wanderPoint = new Vector2(random.Next(wanderRange, GameSettings.ScreenWidth - wanderRange), wallDistance);
                if (wanderSide == 3)
                    wanderPoint = new Vector2(random.Next(wanderRange, GameSettings.ScreenWidth - wanderRange), GameSettings.ScreenHeight - wallDistance);
                FindNextMovement();

                stateSpeed = wanderSpeed;
            break;

            case SheepState.Fleeing:
                stateSpeed = fleeSpeed;
            break;
        }
    }

    private void FindNextMovement()
    {
        switch (state)
        {
            case SheepState.Idle:
                int chosenMode = random.Next(10);

                if (chosenMode <= 5)
                {
                    // Find new idle location
                    Vector2 idleGoal = new Vector2(random.Next(GameSettings.PenLeft, GameSettings.PenRight),
                        random.Next(GameSettings.PenTop, GameSettings.PenBottom));
                    float travelDistance = (idleGoal - Position).Length();
                    stateTimer = travelDistance / idleSpeed;
                    stateDirection = (idleGoal - Position) / travelDistance; 
                }
                else if (chosenMode <= 10 - idleTicksSpent)
                {
                    stateTimer = random.Next(4);
                    stateDirection = Vector2.Zero;
                }
                else
                {
                    EnterNewState(SheepState.Wandering);
                }

                idleTicksSpent++;
            break;

            case SheepState.Wandering:            
                Vector2 wanderGoal = wanderPoint + new Vector2(random.Next(-wanderRange, wanderRange), random.Next(-wanderRange, wanderRange));
                float wanderDistance = (wanderGoal - Position).Length();
                stateDirection = (wanderGoal - Position) / wanderDistance;
                stateTimer = wanderDistance / wanderSpeed;
            break;

            case SheepState.Fleeing:
                EnterNewState(InPen() ? SheepState.Idle : SheepState.Wandering);
            break;
        }
    }

    private Vector2 GetGoal()
    {
        return Position + stateDirection * stateTimer * stateSpeed;
    }

    private void ProcessIdle(float delta)
    {
    }

    private void ProcessFleeing(float delta)
    {
        // Fleeing decrements faster in the pen
        stateTimer -= InPen() ? delta * 3 : 0;
    }

    private void ProcessWandering(float delta)
    {
    }

    protected override void ProcessDreaming(float delta)
    {
    }

#if DEBUG
    public override void _Draw()
    {
        base._Draw();
        if (GameManager.DebugDraw)
        {
            Color debugColor = state == SheepState.Fleeing ? Colors.Red : 
            ( state == SheepState.Idle ? Colors.Blue : Colors.Purple);

            DrawLine(Vector2.Zero, stateDirection * stateSpeed * stateTimer, debugColor);
        }
    }
#endif // #if DEBUG
}
