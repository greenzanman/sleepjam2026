using Godot;
using System;


// Demon that is always active
public class DemonCreeper : Demon
{
    Sprite spriteFront;
    Sprite spriteBack;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        spriteFront = GetNode<Sprite>("SpriteBack");
        spriteBack = GetNode<Sprite>("SpriteFront");
        
        base._Ready();

        travelSpeed = 15;
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
    protected override void UpdateGameState(GameState newGameState)
    {
        targetSheep = null;
        retargetTimer = 0;

        spriteFront.Texture = newGameState == GameState.Awake ? hiddenFront : visibleFront;
        spriteBack.Texture = newGameState == GameState.Dreaming ? hiddenBack : visibleBack;
    }

    protected override void Process(float delta)
    {

        if (targetSheep != null && (targetSheep.cursed || !targetSheep.IsAlive))
            targetSheep = null;


        if (hitTimer > 0)
        {
            hitTimer -= delta;
            SetModulate((int)(hitTimer * 10) % 2 == 1);
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
                    
                    Scale = new Vector2(targetSheep.Position.x > Position.x ? -1 : 1, 1);

                    if (dist < killDistance)
                    {
                        // TODO: Clean this up a bit
                        targetSheep.Bite();
                        Die(); // Creepers only get one
                    }
                }
                else
                {
                    targetSheep = null;
                }

                int frame = (int) (GameManager.GetGameTime() * 3) % 4;
                if (frame == 3) frame = 1;
                spriteFront.Frame = frame;
                spriteBack.Frame = frame;
            }

            SetModulate(currentGameState == GameState.Awake);

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
