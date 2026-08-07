using Godot;
using System;

public class DemonSpawner : SleepNode
{
	private PackedScene demonSneakerScene;
	private PackedScene demonBasicScene;
	private PackedScene demonSheepScene;
	private PackedScene demonCreeperScene;
	private int swaps; // How many day/night swaps have happened
	private Random random = new Random();

	// Tieing to sleep
	private int previousSleepiness; // For callbacks on sleepiness increases

	public override void _Ready()
	{
		demonBasicScene = GD.Load<PackedScene>("res://Demons/DemonBasic.tscn");
		demonSheepScene = GD.Load<PackedScene>("res://Demons/DemonSheep.tscn");
		demonCreeperScene = GD.Load<PackedScene>("res://Demons/DemonCreeper.tscn");
		demonSneakerScene = GD.Load<PackedScene>("res://Demons/DemonSneaker.tscn");
		swaps = 0;
		previousSleepiness = 0;

		// Ready() calls UpdateGameState, so has to go second
		base._Ready();
	
	
	}

	protected override void ProcessAwake(float delta)
	{
		float currentSleepiness = GameManager.GetSleepCount();
		if (currentSleepiness >= previousSleepiness + 1)
		{
			for (int i = previousSleepiness + 1; i < currentSleepiness; i++)
			{
				previousSleepiness+= 1;
				OnSleepiness(previousSleepiness);
			}
		}
	}

	// Callbacks when hitting specific sleepiness
	private void OnSleepiness(int value)
	{
		// On intervals TODO: flesh this out
		if (value < GameManager.MAX_SLEEPCOUNT && value % (GameManager.MAX_SLEEPCOUNT / 4) == 0)
		{
			(Vector2 spawnPoint, int side) = GetEdgeSpawnpoint();

			Demon newDemon = demonBasicScene.Instance<Demon>();
			newDemon.Initialize(side);
			newDemon.Position = spawnPoint;
			GetTree().Root.AddChild(newDemon);
		}
	}

	protected override void UpdateGameState(GameState newGameState)
	{
		swaps ++;

		if (newGameState == GameState.Awake)
		{
			previousSleepiness = 0;
		}

		// Spawns that happen during swaps
		if (newGameState == GameState.Awake && swaps > 1)
		{
				(Vector2 spawnPoint, int side) = GetEdgeSpawnpoint();

				Demon newDemon = demonSheepScene.Instance<Demon>();
				newDemon.Initialize(side);
				newDemon.Position = spawnPoint;
				GetTree().Root.AddChild(newDemon);
		}

		if (newGameState == GameState.Awake && swaps > 1)
		{
				(Vector2 spawnPoint, int side) = GetEdgeSpawnpoint();

				DemonSneaker newDemon = demonSneakerScene.Instance<DemonSneaker>();
				newDemon.Position = spawnPoint;
				newDemon.PrevGameState = newGameState;
				Demon d = newDemon;
				GetTree().Root.AddChild(d);
		}
	}

	const int outsidePadding = 40; // How far outside edges
	const int edgePadding = 100; // How far from corners
	private (Vector2, int) GetEdgeSpawnpoint()
	{
		int spawnSide = random.Next(4);
		if (spawnSide == 0)
			return (new Vector2(-outsidePadding, random.Next(edgePadding, GameSettings.ScreenHeight - edgePadding)), spawnSide);
		else if (spawnSide == 2)
			return (new Vector2(GameSettings.ScreenWidth + outsidePadding, random.Next(edgePadding, GameSettings.ScreenHeight - edgePadding)), spawnSide);
		else if (spawnSide == 1)
			return (new Vector2(random.Next(edgePadding, GameSettings.ScreenWidth - edgePadding), -outsidePadding), spawnSide);
		else
			return (new Vector2(random.Next(edgePadding, GameSettings.ScreenWidth - edgePadding), GameSettings.ScreenHeight + outsidePadding), spawnSide);
	
	}
}
