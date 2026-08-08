using Godot;
using System;

// Not really a demon; obstacle that when overlapped, increases sleepiness and spawns demons
// Can be circular or rectangular
public class DemonSleep : SleepNode
{
    [Export] public bool Circular = true;
    [Export] public int halfLength = 40;
    [Export] public int halfHeight = 100;
    [Export] public int radius = 50;
    private Node2D outerEye;
    private Node2D innerEye;
    private const float innerRadius = 20;
    private const int spawnAmount = 2;
    private const int sleepAmount = 5;

    private float spawnAge = 0;
    private const float spawnDuration = 3;

    private bool triggered = false;
    private float startTriggeredSpeed = -300;
    private float triggerAcceleration = 250;
    public override void _Ready()
    {
        base._Ready();

        outerEye = GetNode<Node2D>("OuterEye");
        innerEye = GetNode<Node2D>("InnerEye");
    
        outerEye.Modulate = GameSettings.colorLight;
        innerEye.Modulate = GameSettings.colorDark;

        if (Circular)
        {
            outerEye.Scale = new Vector2( radius / 10, 0);
        }
        else
        {
            if (halfHeight > halfLength)
                outerEye.Scale = new Vector2( halfLength / 10, 0);
            else
                outerEye.Scale = new Vector2( 0, halfHeight / 10);
        }
    }

    protected override void Process(float delta)
    {
        Vector2 playerPosition = GameManager.GetPlayerWorldPosition().Position;

        if (triggered) // Basic trigger animation
        {
            startTriggeredSpeed += triggerAcceleration * delta;

            float dist;
            (innerEye.GlobalPosition, dist) = Utils.MoveTowardsReturnDistance(innerEye.GlobalPosition, playerPosition, startTriggeredSpeed * delta);

            if (startTriggeredSpeed > 0 && dist < 10)
            {
                QueueFree();
                GameManager.IncrementSleepCount(sleepAmount);
            
                for (int i = 0; i < spawnAmount; i++)
                    DemonSpawner.Instance.SpawnBasicDemon();
                }
            }
        else
        {
            float distance = (playerPosition - Position).Length();

            // Eye positioning
            if (Circular)
            {
                innerEye.Position = (playerPosition - Position) / distance * innerRadius;
            }
            else
            {
                Vector2 unit = (playerPosition - Position) / distance;
                innerEye.Position = new Vector2( unit.x * halfLength, unit.y * halfHeight ) / 2;
            }

            // Eye opening vs collision tracking
            if (spawnAge < spawnDuration)
            {
                spawnAge = Mathf.Min(spawnAge + delta, spawnDuration);

                if (Circular)
                {
                    outerEye.Scale = new Vector2( radius / 10, radius / 10 * spawnAge / spawnDuration);
                }
                else
                {
                    if (halfHeight > halfLength)
                        outerEye.Scale = new Vector2( halfLength / 10, halfHeight / 10 * spawnAge / spawnDuration);
                    else
                        outerEye.Scale = new Vector2( halfLength / 10 * spawnAge / spawnDuration, halfHeight / 10);
                }
            }
            else
            {
                if (Circular)
                {
                    if (distance < radius)
                        Trigger();
                }
                else
                {
                    if (Mathf.Abs(playerPosition.x - Position.x) < halfLength &&
                        Mathf.Abs(playerPosition.y - Position.y) < halfHeight)
                    {
                        Trigger();
                    }
                }
            }
        }
    }

    private void Trigger()
    {
        triggered = true;
        outerEye.Modulate = GameSettings.colorInvisible;
        innerEye.Modulate = GameSettings.colorLight;
    }

    protected override void UpdateGameState(GameState newGameState)
    {
        if (newGameState == GameState.Dreaming)
        {
            QueueFree();
        }
    }

}
