using Godot;
using System;

public class BasicFish : FishBase
{

    private int awakeDirection = 1;
    private int swimSpeed = 50;

    private int health = 2;

    public override void Initialize(Vector2 inWorldPosition, double randVal)
    {
        base.Initialize(inWorldPosition, randVal);

        awakeDirection = randVal > 0.5 ? 1 : -1;
        Scale = new Vector2(-awakeDirection, 1);
    }

    public override void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
            Destroy();
    }


    protected override void ProcessAwake(float delta)
    {
        worldPosition.x += swimSpeed * awakeDirection * delta * 2f;
        
        float borderPadding = 32;
        if (awakeDirection > 0 && worldPosition.x > GameSettings.ScreenWidth - borderPadding)
        {
            awakeDirection = -1;
            Scale = new Vector2(-awakeDirection, 1);
        }

        if (awakeDirection < 0 && worldPosition.x < borderPadding)
        {
            awakeDirection = 1;
            Scale = new Vector2(-awakeDirection, 1);
        }
    }

    protected override void ProcessDreaming(float delta)
    {
        Vector2 playerWorldPosition = GameManager.GetPlayerWorldPosition();

        worldPosition = Utils.MoveTowards(worldPosition, playerWorldPosition, delta * swimSpeed);
    }

}
