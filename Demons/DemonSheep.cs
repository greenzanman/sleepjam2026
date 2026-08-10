using System;
using System.Collections.Generic;
using Godot;

public class DemonSheep : Demon
{
    private bool hasBeenNight = false;
    private Sheep cursedSheep;

    public override void _Ready()
    {
        base._Ready();

        // Stationary version
        // Bitable = false;
        // killDistance = 100;
        // travelSpeed = 0;

        // Normal version
        hp = 4;
        travelSpeed = 25;

        // Find a target sheep
        List<Sheep> validSheep = new List<Sheep>();

        // Prioritize sheep in pen (maybe also prioritize not wandering? rare occurence)
        foreach (Sheep sheep in GameManager.GetSheep())
            if (sheep.IsAlive && !sheep.cursed && !sheep.rebellious && sheep.InPen())
                validSheep.Add(sheep);

        // Backup, choose a wandering sheep
        if (validSheep.Count == 0)
            foreach (Sheep sheep in GameManager.GetSheep())
                if (sheep.IsAlive && !sheep.cursed)
                    validSheep.Add(sheep);
    
        
        // Assumes its day
        if (validSheep.Count <= 1)
        {
            GD.Print("DemonSheep could find no valid targets");
            InPlay = false;
        }
        else
        {
            Random rand = new Random();
            cursedSheep = validSheep[rand.Next(validSheep.Count)];
            cursedSheep.cursed = true;
        }
    }

    protected override void OnDeath()
    {
        if (cursedSheep != null)
            cursedSheep.cursed = false;
        StatKeeper.NumSheepPurified += 1;
    }

    protected override void ProcessDreaming(float delta)
    {
        base.ProcessDreaming(delta);

        if (cursedSheep != null)
            cursedSheep.Position = Position;
    }

    protected override void UpdateGameState(GameState newGameState)
    {
        if (newGameState == GameState.Dreaming)
        {
            hasBeenNight = true;
            if (cursedSheep != null)
            {
                Position = cursedSheep.Position;
            }
            else
            {
                GD.Print("DemonSheep somehow lost cursed sheep");
                Die();
            }
        }
        else if (hasBeenNight)
        {
            // Use death logic for now
            hitTimer = 0.75f;
            Die();
        }

        base.UpdateGameState(newGameState);
    }
}
