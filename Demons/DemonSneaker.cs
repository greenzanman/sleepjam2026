using Godot;
using System;

public class DemonSneaker : Demon
{
	private Polygon2D sheepPolygon { get; set; }
	private Node2D demonPolygons { get; set; }
	private bool canMove { get; set; } = true;
	
	public override void _Ready() 
	{
		base._Ready();
		demonPolygons =  GetNode<Node2D>("DemonPolygons");
		sheepPolygon = GetNode<Polygon2D>("SheepPolygon");
		travelSpeed = 15;
		
		GD.Print("Hello:", sheepPolygon, demonPolygons);
		
		// Modulate = Colors.White;
		sheepPolygon.Modulate = GameSettings.colorLight;
		demonPolygons.Visible = true;
	}
	
	protected override void UpdateGameState(GameState newGameState)
	{
		targetSheep = null;
		retargetTimer = 0;
		
		if (newGameState == GameState.Awake)
		{
			GD.Print("YOOO");
			//Modulate = GameSettings.colorLight;
			sheepPolygon.Modulate = GameSettings.colorLight;
			demonPolygons.Visible = true;
		}
		else
		{
			// Modulate = GameSettings.colorDark;
			sheepPolygon.Modulate = GameSettings.colorDark;
			demonPolygons.Visible = false;
		}
	}
	
	protected override void ProcessAwake(float delta)
	{
		MoveAndRetarget(delta);
	}

	protected override void ProcessDreaming(float delta)
	{
		GD.Print("INPLAY?", InPlay);
		GD.Print("ALIVE?", IsAlive);
		if (hitTimer > 0)
		{
			hitTimer -= delta;
			sheepPolygon.Modulate = (int)(hitTimer * 10) % 2 == 1 ? GameSettings.colorLight : GameSettings.colorDark;
		}
	}
	
	protected override void Process(float delta) 
	{
		base.Process(delta);
		if (!IsAlive)
			InPlay = false;
	}
	
	private void MoveAndRetarget(float delta) {
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
				float dist = 0;
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
	}
}
