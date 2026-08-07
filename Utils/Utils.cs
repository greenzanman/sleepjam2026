using System;
using Godot;

public static class Utils
{
	public static float MoveTowards(float start, float end, float amount)
	{
		float dif = end - start;
		if (amount >= Mathf.Abs(dif)) return end;
		return start + amount * Mathf.Sign(dif);
	}

	public static Vector2 MoveTowards(Vector2 start, Vector2 end, float amount)
	{
		float dist = (end - start).Length();
		if (amount >= dist || dist == 0) return end;
		return start + amount * (end - start) / dist;
	}

	public static (Vector2, float) MoveTowardsReturnDistance(Vector2 start, Vector2 end, float amount)
	{
		float dist = (end - start).Length();
		if (amount >= dist || dist == 0) return (end, 0);
		return (start + amount * (end - start) / dist, dist - amount);
	}
}
