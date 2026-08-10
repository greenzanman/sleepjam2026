using System;
using Godot;

public partial class Demon : SleepNode
{
    private AudioStreamPlayer audioAttack;
    protected float attackVolume = -25.0f;
    
    private AudioStreamPlayer audioConstant;
    protected float constantVolume = -15.0f;
    
    protected float deathVolume = -20.0f;
    
    
    public bool IsAlive = true;
    public bool InPlay = true;
    // Can this enemy be attacked
    public bool Bitable = true;
    protected int spawnSide = 0;
    protected float hp = 2;
    protected float hitTimer = 0;

    protected float killDistance = 50;
    protected float travelSpeed = 50;

    protected Sheep targetSheep;

    protected float retargetTimer = 0;
    
    Random random = new Random();

    protected static Texture hiddenFront = GD.Load<Texture>("res://Demons/wolf-fadeyFront.png");
    protected static Texture hiddenBack = GD.Load<Texture>("res://Demons/wolf-fadeyBack.png");
    protected static Texture visibleFront = GD.Load<Texture>("res://Demons/wolfFront.png");
    protected static Texture visibleBack = GD.Load<Texture>("res://Demons/wolfBack.png");
    public override void _Ready()
    {
        GameManager.AddDemon(this);   
        IsAlive = true; 
        InPlay = true;

        hp = 2;
        audioAttack = new AudioStreamPlayer();
        audioAttack.Stream = GD.Load<AudioStream>("res://Sounds/Wolf Attacking - QuickSounds.com.mp3");
        audioAttack.VolumeDb = attackVolume;
        AddChild(audioAttack);
        
        audioConstant = new AudioStreamPlayer();
        audioConstant.Stream = GD.Load<AudioStream>("res://Sounds/dog-breathing.mp3");
        audioConstant.VolumeDb = constantVolume;
        audioConstant.PitchScale = 0.5f;
        AddChild(audioConstant);
        audioConstant.Play();
        
    }

    public virtual void Initialize(int side)
    {
        spawnSide = side;
    }

    public void Destroy()
    {
        audioConstant.Stop();
        QueueFree();
    }

    public bool IsHurt()
    {
        return hitTimer > 0;
    }
    public void Bite()
    {
        hp -= 1;
        hitTimer = 0.35f;
        if (hp <= 0)
        {
            Die();
        }
    }

    protected void Retarget()
    {
        targetSheep = null;
        float closestDistance = Mathf.Inf;
        foreach (Sheep sheep in GameManager.GetSheep())
        {
            // Don't target dead or cursed sheep
            if (!sheep.IsAlive || sheep.cursed)
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
        if (targetSheep != null && (targetSheep.cursed || !targetSheep.IsAlive))
            targetSheep = null;

            
        if (hitTimer > 0)
        {
            hitTimer -= delta;
            SetModulate((int)(hitTimer * GameSettings.FlashRate) % 2 == 1);
        }
        else
        {
            // When dead and not hit flashing, remove from play
            if (!IsAlive)
            {
                OnDeath();
                InPlay = false;
            }
        }
#if DEBUG
        Update();
#endif
    }

    // Sets hp to 0 and IsAlive to false; also calls OnDeath()
    public virtual void Die() { 
        PlayDeathSound();
        hp = 0; 
        IsAlive = false; 
        OnDeath(); 
    }
    
    private void PlayDeathSound() 
    {
        AudioStreamPlayer deathSound = new AudioStreamPlayer();
        deathSound.Stream = GD.Load<AudioStream>("res://Sounds/dog-shriek.mp3");
        deathSound.VolumeDb = deathVolume;
        deathSound.PitchScale = 0.9f;
        GameManager.WorldRoot.AddChild(deathSound);
        deathSound.PitchScale = 0.8f + (float) random.NextDouble() * 0.3f;
        deathSound.Play();
        deathSound.Connect("finished", deathSound, "queue_free");
    }
    
    // Death logic
    protected virtual void OnDeath() {}
    protected override void ProcessAwake(float delta)
    {
        // Modulate = GameSettings.colorInvisible;
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

                Scale = new Vector2(targetSheep.Position.x > Position.x ? -1 : 1, 1);

                // A bit messy, but better than copying code into DemonClose.cs
                if (this is DemonClose demonClose)
                {
                    if (dist < DemonClose.vulnerabilityDistance)
                        Bitable = true;
                }

                if (dist < killDistance)
                {
                    audioConstant.Stop();
                    audioAttack.Stop();
                    audioAttack.Play();
                    audioConstant.Play();
                    // TODO: Clean this up a bit
                    targetSheep.Bite();

                    // Long pause after killing a sheep, so it doesn't chain
                    retargetTimer = 2;
                }
            }

            SetModulate(false);
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
