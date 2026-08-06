using Godot;

public partial class Demon : SleepNode
{
    public bool IsAlive = true;
    public bool InPlay = true;
    protected int spawnSide = 0;
    protected float hp = 2;
    protected float hitTimer = 0;

    protected float killDistance = 50;
    protected float travelSpeed = 50;

    protected Sheep targetSheep;

    protected float retargetTimer = 0;

    public override void _Ready()
    {
        GameManager.AddDemon(this);   
        IsAlive = true; 
        InPlay = true;

        hp = 2;
    }

    public virtual void Initialize(int side)
    {
        spawnSide = side;
    }

    public void Destroy()
    {
        QueueFree();
    }

    public void Bite()
    {
        hp -= 1;
        hitTimer = 0.35f;
        if (hp <= 0)
        {
            IsAlive = false;
        }
    }

    protected void Retarget()
    {
        targetSheep = null;
        float closestDistance = Mathf.Inf;
        foreach (Sheep sheep in GameManager.GetSheep())
        {
            if (!sheep.IsAlive)
                continue;
                
            float distance = (sheep.Position - Position).Length();
            if (distance < closestDistance)
            {
                targetSheep = sheep;
                closestDistance = distance;
            }
        }

        // Retarget more often if there's no target
        if (targetSheep != null)
        {
            retargetTimer = 0.5f;
        }
        else
        {
            retargetTimer = 0.25f;
        }
    }

    protected override void Process(float delta)
    {
        if (targetSheep != null && !targetSheep.IsAlive)
            targetSheep = null;

            
        if (hitTimer > 0)
        {
            hitTimer -= delta;
            Modulate = (int)(hitTimer * 10) % 2 == 1 ? GameSettings.colorLight : GameSettings.colorDark;
        }
        else
        {
            // When dead and not hit flashing, remove from play
            if (!IsAlive)
                InPlay = false;
        }
#if DEBUG
        Update();
#endif
    }

    protected override void ProcessAwake(float delta)
    {
        Modulate = GameSettings.colorInvisible;
    }

    protected override void ProcessDreaming(float delta)
    {
        if (hitTimer <= 0)
        {
            // Track nearest sheep
            retargetTimer -= delta;
            if (retargetTimer <= 0)
            {
                Retarget();
            }

            if (targetSheep != null)
            {
                float dist = 0;
                (Position, dist) = Utils.MoveTowardsReturnDistance(Position,
                    targetSheep.Position, travelSpeed * delta);
                if (dist < killDistance)
                {
                    // TODO: Clean this up a bit
                    targetSheep.Bite();
                }
            }

            Modulate = GameSettings.colorDark;
        }   
    }

    // Always lightColor
    protected override void UpdateGameState(GameState newGameState)
    {
        Retarget();
    }

#if DEBUG
    public override void _Draw()
    {
        base._Draw();
        if (GameManager.DebugDraw)
        {
            if (targetSheep != null)
            {
                DrawLine(Vector2.Zero, targetSheep.Position - Position, Colors.Red);
            }
        }
    }
#endif // #if DEBUG
}