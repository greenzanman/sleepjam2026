using Godot;
using System;

// Only becomes damagable when it gets close enough to a sheep
public class DemonClose : DemonBasic
{
    public const float vulnerabilityDistance = 160;
    const int angleCount = 64;
    private Vector2[] anglePoints = new Vector2[angleCount + 1];
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();

        hp = 1;
        killDistance = 20; // Much smaller kill distance
        travelSpeed = 40; // Slightly slower

        Bitable = false;

        float radius = vulnerabilityDistance;
        for (int i = 0; i <= angleCount; i++)
        {
            float angle = Mathf.Pi * 2 / angleCount * i;
            anglePoints[i] = new Vector2(radius * -Mathf.Cos(angle), radius * Mathf.Sin(angle));
        }
    }

    protected override void ProcessAwake(float delta)
    {        
        base.ProcessAwake(delta);

        // if (!Bitable)
        // {
        //     // Flicker to show invulnerability
        //     // Modulate = (int)(GameManager.GetGameTime() * 3) % 2 == 1 ? 
        //     //     GameSettings.colorLight : GameSettings.colorDark;
        // }
    }

    protected override void ProcessDreaming(float delta)
    {
        base.ProcessDreaming(delta);
    
        if (hitTimer <= 0)
        {

            spriteFront.Texture = !Bitable ? hiddenFront : visibleFront;
            spriteBack.Texture = !Bitable ? hiddenBack : visibleBack;
        }

        Update();
    }

    // Unbiteableness is set in Demon.cs ProcessDreaming
    public override void _Draw()
    {
        base._Draw();

        if (!Bitable)
        {
            DrawMultiline(anglePoints, GameSettings.colorDark,
                3);
        }
    }

}
