using Godot;
using System;

// Only becomes damagable when it gets close enough to a sheep
public class DemonClose : DemonBasic
{
    public const float vulnerabilityDistance = 250;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();

        hp = 1;
        killDistance = 20; // Much smaller kill distance
        travelSpeed = 40; // Slightly slower

        Bitable = false;
    }

    protected override void ProcessAwake(float delta)
    {
        if (!Bitable)
        {
            // Flicker to show invulnerability
            Modulate = (int)(GameManager.GetGameTime() * 3) % 2 == 1 ? 
                GameSettings.colorLight : GameSettings.colorDark;
        }
    }

    protected override void ProcessDreaming(float delta)
    {
        base.ProcessDreaming(delta);
    
        if (hitTimer <= 0)
        {
            if (!Bitable)
            {
                // Flicker to show invulnerability
                Modulate = (int)(GameManager.GetGameTime() * 3) % 2 == 1 ? 
                    GameSettings.colorLight : GameSettings.colorDark;
            }
            else
            {
                Modulate = GameSettings.colorDark;
            }
        }
    }

    // Unbiteableness is set in Demon.cs ProcessDreaming

}
