using Godot;
using System;


// Demon that is always active
public class DemonCreeper : Demon
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();

        travelSpeed = 15;
    }


    protected override void UpdateGameState(GameState newGameState)
    {
        targetSheep = null;
        retargetTimer = 0;
    }

    protected override void Process(float delta)
    {
        if (hitTimer > 0)
        {
            hitTimer -= delta;
            Modulate = (int)(hitTimer * 10) % 2 == 1 ? GameSettings.colorLight : GameSettings.colorDark;
        }
        else
        {
            // Track nearest sheep
            retargetTimer -= delta;
            if (retargetTimer <= 0)
            {
                Retarget();
            }

            if (targetSheep != null)
            {
                if (targetSheep.IsAlive)
                {
                    float dist;
                    (Position, dist) = Utils.MoveTowardsReturnDistance(Position,
                        targetSheep.Position, travelSpeed * delta);
                    if (dist < killDistance)
                    {
                        // TODO: Clean this up a bit
                        targetSheep.Bite();
                    }
                }
                else
                {
                    targetSheep = null;
                }
            }

            Modulate = Colors.White;

            // When dead and not hit flashing, remove from play
            if (!IsAlive)
                InPlay = false;
        }   

        base.Process(delta);
    }


    protected override void ProcessAwake(float delta)
    {
    }

    protected override void ProcessDreaming(float delta)
    {
    }
}
