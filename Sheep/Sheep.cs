using Godot;
using System;

public class Sheep : SleepNode
{
    private enum SheepState
    {
        Idle,
        Wandering,
        Fleeing
    }

    private SheepState state;

    private bool isDead;
    private Random random = new Random();

    // Fleeing
    float fleeTimer = 0;
    Vector2 fleeDirection;
    float fleeSpeed = 50;

    // Idle
    float idleTimer = 0;
    Vector2 idleDirection;
    float idleSpeed = 40;
    int idleTicksSpent = 0;

    // Wandering
    Vector2 wanderPoint;
    float wanderSpeed = 80;
    float wanderTimer = 0;
    int wanderRange = 100;
    Vector2 wanderDirection;

    // Dreaming
    float hurtTimer = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();

        GameManager.AddSheep(this);
    
        isDead = false;
    }

    public void Bark( Vector2 position)
    {
        Vector2 potentialFleeDirection = Position - position;
        float fleeLength = potentialFleeDirection.Length();
        if (fleeLength < 250)
        {
            fleeTimer = 5 - fleeLength / 75;
            fleeDirection = Mathf.IsZeroApprox(fleeLength) ? Vector2.Left : potentialFleeDirection / fleeLength;
            state = SheepState.Fleeing;
        }
    }

    public void Bite()
    {
        hurtTimer = 0.2f;
    }

    protected virtual void Destroy()
    {
        GameManager.RemoveSheep(this);
        QueueFree();
    }

    protected virtual bool InPen()
    {
        return Position.x > GameSettings.PenLeft && Position.x < GameSettings.PenRight &&
            Position.y > GameSettings.PenTop && Position.y < GameSettings.PenBottom;
    }

    protected override void Process(float delta)
    {
        if (isDead)
            Destroy();

        // TODO: Avoidance

#if DEBUG
        Update();
#endif
    }

    protected override void ProcessAwake(float delta)
    {
        bool wasInPen = InPen();

        switch (state)
        {
            // Either move to another spot in the pen, stand still, or start wandering
            case SheepState.Idle:
                ProcessIdle(delta);
            break;
            // Flee from bark source
            case SheepState.Fleeing:
                Position += fleeDirection * fleeSpeed * delta; // TODO: Share this travel logic
                fleeTimer -= InPen() ? delta * 4 : delta;

                if (fleeTimer <= 0)
                {
                    state = InPen() ? SheepState.Idle : SheepState.Wandering;
                    idleTimer = 0;
                    idleTicksSpent = 0;
                }
            break;
            // Idle around a point outside the region
            case SheepState.Wandering:
                wanderTimer -= delta;
                Position += wanderDirection * wanderSpeed * delta;
                if (wanderTimer <= 0)
                {
                    Vector2 wanderGoal = wanderPoint + new Vector2(random.Next(-wanderRange, wanderRange), random.Next(-wanderRange, wanderRange));
                    float distance = (wanderGoal - Position).Length();
                    wanderDirection = (wanderGoal - Position) / distance;
                    wanderTimer = distance / wanderSpeed;
                }
            break;
        }
    
        // Temp crossing fence code
        if (InPen() != wasInPen)
        {
            GameManager.IncrementSleepCount();
        }
    }

    private void ProcessIdle(float delta)
    {
        idleTimer -= delta;
        Position += delta * idleDirection * idleSpeed;
        if (idleTimer <= 0)
        {
            int chosenMode = random.Next(10);

            if (chosenMode <= 5)
            {
                // Find new idle location
                Vector2 idleGoal = new Vector2(random.Next(GameSettings.PenLeft, GameSettings.PenRight),
                    random.Next(GameSettings.PenTop, GameSettings.PenBottom));
                float distance = (idleGoal - Position).Length();
                idleTimer = distance / idleSpeed;
                idleDirection = (idleGoal - Position) / distance; 
            }
            else if (chosenMode <= 12 - idleTicksSpent)
            {
                idleTimer = random.Next(4);
                idleDirection = Vector2.Zero;
            }
            else
            {
                // TODO: Move this into a "enter state" function
                state = SheepState.Wandering;
                wanderTimer = 0;
                int wanderSide = random.Next(4);
                // Choose random wanter point on outside edges
                int wallDistance = random.Next(wanderRange, wanderRange * 2);
                if (wanderSide == 0)
                    wanderPoint = new Vector2(wallDistance, random.Next(wanderRange, GameSettings.ScreenHeight - wanderRange));
                if (wanderSide == 2)
                    wanderPoint = new Vector2(wallDistance, random.Next(GameSettings.ScreenWidth - wanderRange, GameSettings.ScreenHeight - wanderRange));
                if (wanderSide == 1)
                    wanderPoint = new Vector2(random.Next(wanderRange, GameSettings.ScreenWidth - wanderRange), wallDistance);
                if (wanderSide == 3)
                    wanderPoint = new Vector2(random.Next(wanderRange, GameSettings.ScreenWidth - wanderRange), GameSettings.ScreenHeight - wallDistance);
            }

            idleTicksSpent++;
        }
    }

    protected override void ProcessDreaming(float delta)
    {
        hurtTimer -= delta;
        if (hurtTimer >= 0)
        {
            Modulate = (int)(hurtTimer * 20) % 2 == 1 ? GameSettings.colorLight : GameSettings.colorDark;
        }
        else
        {
            Modulate = GameSettings.colorDark;
        }
    }

#if DEBUG
    public override void _Draw()
    {
        base._Draw();
        if (GameManager.DebugDraw)
        {
            if (state == SheepState.Fleeing)
            {
                DrawLine( Vector2.Zero, fleeDirection * fleeSpeed * fleeTimer,
                    Colors.Red);
            }
            if (state == SheepState.Idle)
            {
                DrawLine( Vector2.Zero, idleDirection * idleSpeed * idleTimer,
                    Colors.Blue);
            }
            if (state == SheepState.Wandering)
            {
                DrawLine( Vector2.Zero, wanderDirection * wanderTimer * wanderSpeed,
                    Colors.Purple);
            }
        }
    }
#endif // #if DEBUG
}
