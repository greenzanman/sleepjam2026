using Godot;
using System;

// Displays a warning marking and disappears on daytime
public class DemonBasic : Demon
{
    protected Sprite spriteFront;
    protected Sprite spriteBack;
    private bool hasBeenNight = false;

    private Node2D spawnIndicator;
    private const float spawnOffset = 100;
    private Vector2 spawnIndicatorOffset;

    public override void _Ready()
    {
        spriteFront = GetNode<Sprite>("SpriteBack");
        spriteBack = GetNode<Sprite>("SpriteFront");
        spawnIndicator = GetNode<Node2D>("SpawnIndicator");
        base._Ready();

    }
    protected override void SetModulate(bool awake)
    {
        if (awake)
        {
            spriteFront.Modulate = GameSettings.colorLight;
            spriteBack.Modulate = GameSettings.colorDark;
            spawnIndicator.Modulate = GameSettings.colorLight;
        }
        else
        {
            spriteFront.Modulate = GameSettings.colorDark;
            spriteBack.Modulate = GameSettings.colorLight;
            spawnIndicator.Modulate = GameSettings.colorDark;
        }
    }

    public override void Initialize(int side)
    {
        base.Initialize(side);

        spawnIndicatorOffset = new Vector2(
            side % 2 == 0 ? spawnOffset * (1 - side): 0,
            side % 2 == 1 ? spawnOffset * (2 - side) : 0);
    }

    protected override void UpdateGameState(GameState newGameState)
    {
        if (newGameState == GameState.Dreaming)
        {
            hasBeenNight = true;
        }
        else if (hasBeenNight) // Die upon waking up
        {
            // Use death logic for now
            hitTimer = 0.75f;
            Die();
        }

        // Hode spawn indicator
        if (hasBeenNight)
        {
            spawnIndicator.Visible = false;
        }

        base.UpdateGameState(newGameState);
    }

    protected override void Process(float delta)
    {
        base.Process(delta);

        if (hitTimer <= 0)
        {
            int frame = (int) (GameManager.GetGameTime() * 3) % 4;
            if (frame == 3) frame = 1;
            spriteFront.Frame = frame;
            spriteBack.Frame = frame;
        }
    }

    protected override void ProcessAwake(float delta)
    {
        // TODO: Display spawn indicator instead of modulate
        if (!hasBeenNight)
        {
            SetModulate(true);
            spawnIndicator.Position = spawnIndicatorOffset + 
                15 * Vector2.Up * Mathf.Sin(GameManager.GetGameTime() * 3);
        }
    }   
}
