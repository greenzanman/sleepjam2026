using Godot;
using System;
using System.Collections.Generic;

public class Player : SleepNode
{

	private Vector2 velocity;
	private const float ACCELERATION = 5000;
	private const float MAXSPEED = 400;

	// Awake state
	const float barkCooldown = 0.5f;
	const float fakeBarkBuffer = 0.05f; // Bar is slightly slower so it 'feels' better

	float barkTimer = 0;

	public override void _Ready()
	{
		base._Ready();
		GameManager.SetPlayer(this);
	}

	protected override void Process(float delta)
	{
		ProcessMovement(delta);

		Update(); // TODO: Build this into sprite or something
	}

	private void ProcessMovement(float delta)
	{
		
		Vector2 movementInput = new Vector2(Input.GetAxis("key_left", "key_right"),
			Input.GetAxis("key_up", "key_down"));

		if (movementInput != Vector2.Zero)
		{
			movementInput = movementInput.Normalized();
		}

		velocity = Utils.MoveTowards(velocity, movementInput * MAXSPEED, ACCELERATION * delta);
		Position += velocity * delta;
		// TODO: Keep within boundaries

	}

	protected override void ProcessAwake(float delta)
	{
		barkTimer = Mathf.Max(-10, barkTimer - delta);
		if (barkTimer <= 0 && Input.IsActionJustPressed("key_action"))
		{
			foreach (Sheep sheep in GameManager.GetSheep())
			{
				sheep.Bark(Position);
			}

			barkTimer = barkCooldown;
		}
	}

	protected override void ProcessDreaming(float delta)
	{
		Demon closestDemon = null;
		float closestDistance = 100;
		if (Input.IsActionJustPressed("key_action"))
		{
			// TODO: Improve performance of all these iteration distance checks
			foreach (Demon demon in GameManager.GetDemons())
			{
				if (!demon.IsAlive || !demon.Bitable)
					continue;
					
				float distance = (demon.Position - Position).Length();
				if (distance < closestDistance)
				{
					closestDistance = distance;
					closestDemon = demon;
				}
			}
		}
		if (closestDemon != null)
		{
			closestDemon.Bite();
		}
	}

	public override void _Draw()
	{
		base._Draw();
		float fill = 60 * Mathf.Max(barkTimer + fakeBarkBuffer, 0) / (barkCooldown + fakeBarkBuffer);
		DrawLine(new Vector2(-30, -30), new Vector2(-30 + fill, -30), Colors.Purple, 5);
	}
}
