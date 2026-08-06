using Godot;
using System;

public class DemonSneaker : Demon
{
	private Polygon2D sheepPolygon { get; set; }
	private Node2D demonPolygons { get; set; }
	private bool mutated = false;
	public GameState PrevGameState {private get; set; }
	
	public override void _Ready() 
	{
		base._Ready();
		demonPolygons =  GetNode<Node2D>("DemonPolygons");
		sheepPolygon = GetNode<Polygon2D>("SheepPolygon");
		travelSpeed = 15;
		
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
			//Modulate = GameSettings.colorLight;
			Disguise();
			
			if(PrevGameState == GameState.Dreaming) 
			{
				Mutate();
			}
		}
		else if (mutated == false)
		{
			// Modulate = GameSettings.colorDark;
			NoDisguise();
		}
		PrevGameState = newGameState;
	}
	
	protected override void ProcessAwake(float delta)
	{
		MoveAndRetarget(delta);
	}

	protected override void ProcessDreaming(float delta)
	{
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
					if(!targetSheep.IsAlive) {
						InPlay = false;
						IsAlive = false;
					}
				}
			}
			else
			{
				targetSheep = null;
			}
		}	
	}
	
	private void Disguise() 
	{
		sheepPolygon.Modulate = GameSettings.colorLight;
		demonPolygons.Visible = true;
	}
	
	private void NoDisguise() 
	{
		sheepPolygon.Modulate = GameSettings.colorDark;
		demonPolygons.Visible = false;
	}
	
	private void Mutate() 
	{
		mutated = true;
		travelSpeed *= 30;
		Scale *= 3;
	}
}
