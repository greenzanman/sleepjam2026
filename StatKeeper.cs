using Godot;
using System;

public class StatKeeper : Node
{
	// Number of demons kiled.
	public static int NumDemonsKilled { get; set; }
	
	// Number of times a sheep has jumped over a fence.
	public static int NumFenceJumps { get; set; }
	
	// Number of times a sheep has died.
	public static int NumSheepDeaths { get; set; }
	
	// Number of times 'dreaming' phases have occured.
	public static int NumSleepInstances { get; set; }
	
	// Number of times you turn a cursed sheep back to normal.
	public static int NumSheepPurified { get; set; }
	
	// Number of times the player barks to herd sheep.
	public static int NumBarks { get; set; }
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ResetStats();
	}
	
	public override void _Process(float delta) 
	{
		//PrintStats();
	}
	
	public static void ResetStats()
	{
		NumDemonsKilled = 0;
		NumFenceJumps = 0;
		NumSheepDeaths = 0;
		NumSleepInstances = 0;
		NumSheepPurified = 0;
	}
	
	public static void PrintStats()
	{
		GD.Print($@"
			--- START ---
			DemonsKilled: {NumDemonsKilled}
			FenceJumps: {NumFenceJumps}
			SheepDeaths: {NumSheepDeaths}
			SleepInstances: {NumSleepInstances}
			SheepPurified: {NumSheepPurified}
			--- END ---
		");
	}
}
