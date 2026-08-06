using Godot;
using System;

// Displays a warning marking and disappears on daytime
public class DemonBasic : Demon
{
    private bool hasBeenNight = false;

    private Node2D spawnIndicator;
    private const float spawnOffset = 100;
    private Vector2 spawnIndicatorOffset;

    public override void _Ready()
    {
        base._Ready();

        spawnIndicator = GetNode<Node2D>("SpawnIndicator");
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
            hp = 0;
            IsAlive = false;
        }

        // Hode spawn indicator
        if (hasBeenNight)
        {
            spawnIndicator.Visible = false;
        }

        base.UpdateGameState(newGameState);
    }

    protected override void ProcessAwake(float delta)
    {
        // TODO: Display spawn indicator instead of modulate
        if (!hasBeenNight)
        {
            Modulate = GameSettings.colorLight;
            spawnIndicator.Position = spawnIndicatorOffset + 
                15 * Vector2.Up * Mathf.Sin(GameManager.GetGameTime() * 3);
        }
    }   
}